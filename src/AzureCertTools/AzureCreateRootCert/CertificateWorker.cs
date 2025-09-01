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

namespace CertTools.AzureCreateRootCert;

/// <summary>
/// Class providing the functionality to create a self signed root certificate and upload it to Azure Key Vault.
/// </summary>
internal static class CertificateWorker
{
   /// <summary>Extended Key Usage OID for Code Signing</summary>
   private const string CodeSigningEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.3";

   /// <summary>Extended Key Usage OID for Code Signing friendly name</summary>
   private const string CodeSigningEnhancedKeyUsageOidFriendlyName = "Code Signing";

   /// <summary>
   /// Create a self signed root certificate with the given name, subject name and store it in Azure Key Vault.
   /// </summary>
   /// <param name="certificateName">The Azure Key Vault certificate name.</param>
   /// <param name="subjectNameValue">The subject name for the certificate.</param>
   /// <param name="expireMonth">The number of month until the certificate expires.</param>
   /// <param name="vaultUri">The URI to the Azure Key Vault.</param>
   /// <param name="tokenCredential">The Azure Key Vault token credential.</param>
   public static async Task<string> CreateRootCertAsync(string certificateName, string subjectNameValue, int expireMonth, Uri vaultUri, TokenCredential tokenCredential)
   {
      var client = new CertificateClient(vaultUri, tokenCredential);

      // create a temporary self signed cert in Azure Key Vault, only used to sign a single CSR
      var signatureGenerator = await CertificateWorkerCore.KeyVaultCreateSelfSignedCertAsync(certificateName, subjectNameValue, client, tokenCredential, false, 1);
      var signerName = new X500DistinguishedName(subjectNameValue);
      
      // create a CSR and sign it with the temporary cert
      var csr = await KeyVaultCreateRootCertificateRequestAsync(certificateName, subjectNameValue, client, expireMonth);
      using var certificate = CertificateWorkerCore.SignCertificateRequest(csr, signerName, signatureGenerator, expireMonth);

      // Merge with the pending certificate operation
      await CertificateWorkerCore.KeyVaultMergeCertificateAsync(certificateName, certificate, client);

      return certificateName;
   }

   private static async Task<CertificateRequest> KeyVaultCreateRootCertificateRequestAsync(string certificateName, string subjectNameValue, CertificateClient client, int expireMonth)
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

      certSigningRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 0, true));
      certSigningRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.DigitalSignature, false));
      certSigningRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certSigningRequest.PublicKey, false));
      certSigningRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid(CodeSigningEnhancedKeyUsageOid, CodeSigningEnhancedKeyUsageOidFriendlyName)], true));

      return certSigningRequest;
   }
}
