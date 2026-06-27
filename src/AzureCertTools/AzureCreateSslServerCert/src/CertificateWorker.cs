// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using CertTools.AzureCertCore;

namespace CertTools.AzureCreateSslServerCert;

/// <summary>
/// Class providing the functionality to create a SSL Server certificate locally or in Azure Key Vault.
/// </summary>
internal static class CertificateWorker
{
   /// <summary>
   /// Create SSL server certificate as an asynchronous operation.
   /// </summary>
   /// <param name="certificateName">Name of the certificate.</param>
   /// <param name="fullQualifiedDomainName">The FQDN of the server endpoint.</param>
   /// <param name="signerCertificateName">Name of the signer certificate.</param>
   /// <param name="vaultUri">The vault URI.</param>
   /// <param name="tokenCredential">The token credential.</param>
   /// <param name="expireMonths">The expire months.</param>
   /// <param name="local">if set to <c>true</c> [local].</param>
   /// <param name="password">The password.</param>
   /// <param name="keyOptions">The key creation options controlling the key type, size and exportability.</param>
   /// <param name="signerVaultUri">The URI of the Key Vault holding the signer certificate. If null, <paramref name="vaultUri"/> is used.</param>
   /// <returns>A Task&lt;System.String&gt; representing the asynchronous operation.</returns>
   public static async Task<string> CreateSslServerCertificateAsync(string certificateName, string fullQualifiedDomainName, string signerCertificateName, Uri vaultUri, TokenCredential tokenCredential, int expireMonths, bool local, string? password, KeyCreationOptions keyOptions, Uri? signerVaultUri = null)
   {
      if (local)
      {
         ArgumentNullException.ThrowIfNull(password, nameof(password));

         var signerClient = new CertificateClient(signerVaultUri ?? vaultUri, tokenCredential);

         // Get the signer certificate and its associated keys
         (var signerName, var signerSignaturGenerator) = await CertificateWorkerCore.KeyVaultGetSignerCertificateAsync(signerCertificateName, signerClient, tokenCredential);

         // create a CSR
         var csr = await LocalCreateCertificateRequestAsync(fullQualifiedDomainName, keyOptions);

         // Sign the CSR
         using var certificate = CertificateWorkerCore.SignCertificateRequest(csr, signerName, signerSignaturGenerator, expireMonths);

         // Export the certificate to a PFX file
         var pfxContents = certificate.Export(X509ContentType.Pfx, password);
         await File.WriteAllBytesAsync(certificateName + ".pfx", pfxContents);

         return $"filename={certificateName}.pfx";
      }
      else
      {
         var client = new CertificateClient(vaultUri, tokenCredential);
         var signerClient = signerVaultUri != null ? new CertificateClient(signerVaultUri, tokenCredential) : client;
         
         // Get the signer certificate and its associated keys
         (var signerName, var signerSignaturGenerator) = await CertificateWorkerCore.KeyVaultGetSignerCertificateAsync(signerCertificateName, signerClient, tokenCredential);
         
         // create a CSR
         var csr = await KeyVaultCreateCertificateRequestAsync(certificateName, fullQualifiedDomainName, client, expireMonths, keyOptions);
         
         // Sign the CSR
         var cert = CertificateWorkerCore.SignCertificateRequest(csr, signerName, signerSignaturGenerator, expireMonths);
         
         // and upload it to Key Vault
         await CertificateWorkerCore.KeyVaultMergeCertificateAsync(certificateName, cert, client);

         return $"name={certificateName}";
      }
   }

   private static async Task<CertificateRequest> KeyVaultCreateCertificateRequestAsync(string certificateName, string fullQualifiedDomainName, CertificateClient client, int expireMonth, KeyCreationOptions keyOptions)
   {
      var subjectNameValue = "CN=" + fullQualifiedDomainName;
      var certificatePolicy = CertificateWorkerCore.CreateCertificatePolicy(WellKnownIssuerNames.Unknown, subjectNameValue, expireMonth, keyOptions);

      // Stage 1: Create the certificate, the operation will not be completed yet
      _ = await client.StartCreateCertificateAsync(certificateName, certificatePolicy);
      
      // Stage 2: Get the certificate operation, we need the CSR to sign
      var certificateOperation = await client.GetCertificateOperationAsync(certificateName);

      // Stage 3: Get the CSR from the certificate operation
      var certOperationCertSigningRequest = certificateOperation.Properties.Csr;

      var (signerHashAlgorithm, signerSignaturePadding) = keyOptions switch
      {
         EcKeyCreationOptions ecOptions => (GetEcSignerHashAlgorithm(ecOptions.KeyCurve), null),
         RsaKeyCreationOptions => (HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1),
         _ => throw new NotSupportedException($"Unsupported key type '{keyOptions.GetType()}'."),
      };

      // Stage 4: Get the .NET CSR object
      var certSigningRequest = CertificateRequest.LoadSigningRequest(pkcs10: certOperationCertSigningRequest,
         signerHashAlgorithm: signerHashAlgorithm, signerSignaturePadding: signerSignaturePadding);

      // Stage 5: Add required extensions for a SSL Server certificate
      await AddCertificateExtensionsAsync(certSigningRequest, fullQualifiedDomainName);

      return certSigningRequest;
   }

   [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Tools are Windows only")]
   [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The asymmetric key is intentionally retained by the certificate request for the subsequent local signing flow.")]
   private static async Task<CertificateRequest> LocalCreateCertificateRequestAsync(string fullQualifiedDomainName, KeyCreationOptions keyOptions)
   {
      ArgumentNullException.ThrowIfNull(keyOptions);

      var distinguishedName = "CN=" + fullQualifiedDomainName;

      // Create the CSR
      var subjectName = new X500DistinguishedName(distinguishedName);
      var certSigningRequest = keyOptions switch
      {
         EcKeyCreationOptions ecOptions => new CertificateRequest(
            subjectName,
            ECDsa.Create(ecOptions.GetEcCurve()),
            CertificateWorkerCore.GetHashAlgorithmName(ecOptions)),
         RsaKeyCreationOptions rsaOptions => new CertificateRequest(
            subjectName,
            CreateExportableRsaKey(rsaOptions.KeySize),
            CertificateWorkerCore.GetHashAlgorithmName(rsaOptions),
            CertificateWorkerCore.GetRSASignaturePadding(rsaOptions)!),
         _ => throw new NotSupportedException($"Unsupported key type '{keyOptions.GetType()}'."),
      };

      // Add required extensions for a SSL Server certificate
      await AddCertificateExtensionsAsync(certSigningRequest, fullQualifiedDomainName);

      return certSigningRequest;
   }

   [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Tools are Windows only")]
   [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Not relevant as this ie a one time only operation")]
   private static RSA CreateExportableRsaKey(int keySize)
   {
      var cspParameter = new CspParameters();
      cspParameter = new CspParameters(cspParameter.ProviderType, cspParameter.ProviderName, Guid.NewGuid().ToString())
      {
         Flags = CspProviderFlags.UseArchivableKey,
      };

      return new RSACryptoServiceProvider(keySize, cspParameter);
   }

   private static async Task AddCertificateExtensionsAsync(CertificateRequest certSigningRequest, string FQDN)
   {
      var sanBuilder = new SubjectAlternativeNameBuilder();
      sanBuilder.AddDnsName(FQDN);

      try
      {
         var hostEntry = await Dns.GetHostEntryAsync(FQDN);
         if (!string.Equals(hostEntry.HostName, FQDN, StringComparison.OrdinalIgnoreCase))
         {
            sanBuilder.AddDnsName(hostEntry.HostName);
         }
      }
      catch (SocketException)
      {
      }

      certSigningRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
      certSigningRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, true));
      certSigningRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certSigningRequest.PublicKey, false));
      certSigningRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid(Constants.ServerAuthenticationEnhancedKeyUsageOid, Constants.ServerAuthenticationEnhancedKeyUsageOidFriendlyName)], false));
      certSigningRequest.CertificateExtensions.Add(sanBuilder.Build(true));
      certSigningRequest.CertificateExtensions.Add(new X509Extension(new AsnEncodedData(
         new Oid(Constants.AspNetHttpsEnhancedKeyUsageOid, Constants.AspNetHttpsEnhancedKeyUsageOidFriendlyName), [Constants.AspNetCurrentCertificateVersion]), false));
   }

   private static HashAlgorithmName GetEcSignerHashAlgorithm(EcKeyCurve keyCurve) => keyCurve switch
   {
      EcKeyCurve.P256 or EcKeyCurve.P256K => HashAlgorithmName.SHA256,
      EcKeyCurve.P521 => HashAlgorithmName.SHA512,
      _ => HashAlgorithmName.SHA384,
   };
}
