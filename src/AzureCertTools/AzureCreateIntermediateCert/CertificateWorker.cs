// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using CertTools.AzureCertCore;

namespace CertTools.AzureCreateIntermediateCert;

/// <summary>
/// Class providing the functionality to create a intermediate CA certificate and upload it to Azure Key Vault.
/// </summary>
internal static class CertificateWorker
{
   /// <summary>
   /// Create a self signed root certificate with the given name, subject name and store it in Azure Key Vault.
   /// </summary>
   /// <param name="certificateName">The Azure Key Vault certificate name.</param>
   /// <param name="subjectNameValue">The subject name for the certificate.</param>
   /// <param name="expireMonths">The number of month until the certificate expires.</param>
   /// <param name="vaultUri">The URI to the Azure Key Vault.</param>
   /// <param name="tokenCredential">The Azure Key Vault token credential.</param>
   public static async Task<string> CreateIntermediateCertAsync(string certificateName, string subjectNameValue, string signerCertificateName, int expireMonths, Uri vaultUri, TokenCredential tokenCredential)
   {
      var client = new CertificateClient(vaultUri, tokenCredential);

      // Get the signer certificate and its associated keys
      (var signerName, var signerSignaturGenerator) = await CertificateWorkerCore.KeyVaultGetSignerCertificateAsync(signerCertificateName, client, tokenCredential);

      // create a CSR
      var csr = await KeyVaultCreateCertificateRequestAsync(certificateName, subjectNameValue, client, expireMonths);

      // Sign the CSR
      var cert = CertificateWorkerCore.SignCertificateRequest(csr, signerName, signerSignaturGenerator, expireMonths);

      // and upload it to Key Vault
      await CertificateWorkerCore.KeyVaultMergeCertificateAsync(certificateName, cert, client);

      return certificateName;
   }

   private static async Task<CertificateRequest> KeyVaultCreateCertificateRequestAsync(string certificateName, string subjectNameValue, CertificateClient client, int expireMonth)
   {
      var certificatePolicy = new CertificatePolicy(WellKnownIssuerNames.Unknown, subjectNameValue)
      {
         KeyType = CertificateKeyType.Rsa,
         KeySize = CertificateWorkerCore.RsaKeySize,
         ReuseKey = true,
         ValidityInMonths = expireMonth,
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

      // Stage 5: Add required extensions for a CA certificate
      certSigningRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 0, true));
      certSigningRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.DigitalSignature, false));
      certSigningRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certSigningRequest.PublicKey, false));
      certSigningRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([
         new Oid(Constants.ServerAuthenticationEnhancedKeyUsageOid, Constants.ServerAuthenticationEnhancedKeyUsageOidFriendlyName),
         new Oid(Constants.ClientAuthenticationEnhancedKeyUsageOid, Constants.ClientAuthenticationEnhancedKeyUsageOidFriendlyName),
         new Oid(Constants.CodeSigningEnhancedKeyUsageOid, Constants.CodeSigningEnhancedKeyUsageOidFriendlyName)
      ], true));

      return certSigningRequest;
   }
}
