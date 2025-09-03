// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
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
   /// <returns>A Task&lt;System.String&gt; representing the asynchronous operation.</returns>
   public static async Task<string> CreateSslServerCertificateAsync(string certificateName, string fullQualifiedDomainName, string signerCertificateName, Uri vaultUri, TokenCredential tokenCredential, int expireMonths, bool local, string? password)
   {
      if (local)
      {
         ArgumentNullException.ThrowIfNull(password, nameof(password));

         var client = new CertificateClient(vaultUri, tokenCredential);

         // Get the signer certificate and its associated keys
         (var signerName, var signerSignaturGenerator) = await CertificateWorkerCore.KeyVaultGetSignerCertificateAsync(signerCertificateName, client, tokenCredential);

         // create a CSR
         var csr = await LocalCreateCertificateRequestAsync(fullQualifiedDomainName);

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
         
         // Get the signer certificate and its associated keys
         (var signerName, var signerSignaturGenerator) = await CertificateWorkerCore.KeyVaultGetSignerCertificateAsync(signerCertificateName, client, tokenCredential);
         
         // create a CSR
         var csr = await KeyVaultCreateCertificateRequestAsync(certificateName, fullQualifiedDomainName, client, expireMonths);
         
         // Sign the CSR
         var cert = CertificateWorkerCore.SignCertificateRequest(csr, signerName, signerSignaturGenerator, expireMonths);
         
         // and upload it to Key Vault
         await CertificateWorkerCore.KeyVaultMergeCertificateAsync(certificateName, cert, client);

         return $"name={certificateName}";
      }
   }

   private static async Task<CertificateRequest> KeyVaultCreateCertificateRequestAsync(string certificateName, string fullQualifiedDomainName, CertificateClient client, int expireMonth)
   {
      var subjectNameValue = "CN=" + fullQualifiedDomainName;
      var certificatePolicy = new CertificatePolicy(WellKnownIssuerNames.Unknown, subjectNameValue)
      {
         KeyType = CertificateKeyType.Rsa,
         KeySize = CertificateWorkerCore.RsaKeySize,
         ReuseKey = true,
         Exportable = true,
         ValidityInMonths = expireMonth
      };

      // Stage 1: Create the certificate, the operation will not be completed yet
      _ = await client.StartCreateCertificateAsync(certificateName, certificatePolicy);
      
      // Stage 2: Get the certificate operation, we need the CSR to sign
      var certificateOperation = await client.GetCertificateOperationAsync(certificateName);

      // Stage 3: Get the CSR from the certificate operation
      var certOperationCertSigningRequest = certificateOperation.Properties.Csr;

      // Stage 4: Get the .NET CSR object
      var certSigningRequest = CertificateRequest.LoadSigningRequest(pkcs10: certOperationCertSigningRequest,
         signerHashAlgorithm: HashAlgorithmName.SHA384, signerSignaturePadding: RSASignaturePadding.Pkcs1);

      // Stage 5: Add required extensions for a SSL Server certificate
      await AddCertificateExtensionsAsync(certSigningRequest, fullQualifiedDomainName);

      return certSigningRequest;
   }

   private static async Task<CertificateRequest> LocalCreateCertificateRequestAsync(string fullQualifiedDomainName)
   {
      var distinguishedName = "CN=" + fullQualifiedDomainName;

      // Create a new RSA key and wrap it in a CSP parameters to make it exportable 
      var cspParameter = new CspParameters();
      cspParameter = new CspParameters(cspParameter.ProviderType, cspParameter.ProviderName, Guid.NewGuid().ToString())
      {
         Flags = CspProviderFlags.UseArchivableKey
      };
      using var rsaKeyPair = new RSACryptoServiceProvider(CertificateWorkerCore.RsaKeySize, cspParameter);

      // Create the CSR
      var subjectName = new X500DistinguishedName(distinguishedName);
      var certSigningRequest = new CertificateRequest(subjectName, rsaKeyPair, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);

      // Add required extensions for a SSL Server certificate
      await AddCertificateExtensionsAsync(certSigningRequest, fullQualifiedDomainName);

      return certSigningRequest;
   }

   private static async Task AddCertificateExtensionsAsync(CertificateRequest certSigningRequest, string FQDN)
   {
      var hostEntry = await Dns.GetHostEntryAsync(FQDN);
      var sanBuilder = new SubjectAlternativeNameBuilder();
      sanBuilder.AddDnsName(FQDN);
      sanBuilder.AddDnsName(hostEntry.HostName);

      certSigningRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
      certSigningRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, true));
      certSigningRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certSigningRequest.PublicKey, false));
      certSigningRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid(Constants.ServerAuthenticationEnhancedKeyUsageOid, Constants.ServerAuthenticationEnhancedKeyUsageOidFriendlyName)], false));
      certSigningRequest.CertificateExtensions.Add(sanBuilder.Build(true));
      certSigningRequest.CertificateExtensions.Add(new X509Extension(new AsnEncodedData(
         new Oid(Constants.AspNetHttpsEnhancedKeyUsageOid, Constants.AspNetHttpsEnhancedKeyUsageOidFriendlyName), [Constants.AspNetCurrentCertificateVersion]), false));
   }
}
