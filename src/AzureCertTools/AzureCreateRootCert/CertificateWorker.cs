// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using CertTools.AzureCertCore;

namespace CertTools.AzureCreateRootCert;

/// <summary>
/// Class providing the functionality to create a self signed root certificate and upload it to Azure Key Vault.
/// </summary>
internal class CertificateWorker
{
   /// <summary>The number of months until the root certificate expires.</summary>
   private const int RootCertExpireMonth = 120;

   /// <summary>Extended Key Usage OID for Code Signing</summary>
   private const string CodeSigningEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.3";

   /// <summary>Extended Key Usage OID for Code Signing friendly name</summary>
   private const string CodeSigningEnhancedKeyUsageOidFriendlyName = " Code Signing";

   /// <summary>
   /// Create a self signed root certificate with the given subject name and export it as PFX and CER.
   /// The files are written to the current directory.
   /// </summary>
   /// <param name="subjectNameValue"></param>
   /// <param name="fileName">The filename w/o extension of the cert files</param>
   /// <param name="passwordValue"></param>
   public static async Task<string> CreateRootCertAsync(string certificateName, string subjectNameValue, Uri vaultUri, TokenCredential tokenCredential)
   {
      var client = new CertificateClient(vaultUri, tokenCredential);

      // create a temporary self signed cert in Azure Key Vault, only used to sign a single CSR
      var signatureGenerator = await CertificateWorkerCore.KeyVaultCreateSelfSignedCertAsync(certificateName, subjectNameValue, client, tokenCredential, false, 1);
      var signerName = new X500DistinguishedName(subjectNameValue);
      
      // create a CSR and sign it with the temporary cert
      var csr = await KeyVaultCreateRootCertificateRequestAsync(certificateName, subjectNameValue, client, RootCertExpireMonth);
      using var certificate = CertificateWorkerCore.SignCertificateRequest(csr, signerName, signatureGenerator, RootCertExpireMonth);

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
