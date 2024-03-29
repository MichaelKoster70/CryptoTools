﻿// ----------------------------------------------------------------------------
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

namespace CertTools.AzureCreateSigningCert;

/// <summary>
/// Class providing the functionality to create a code signing certificate in Azure Key Vault.
/// </summary>
internal static class CertificateWorker
{
   /// <summary>Extended Key Usage OID for Code Signing</summary>
   private const string CodeSigningEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.3";

   /// <summary>Extended Key Usage OID for Code Signing friendly name</summary>
   private const string CodeSigningEnhancedKeyUsageOidFriendlyName = " Code Signing";

   /// <summary>
   /// Create a code signing certificate in Azure Key Vault.
   /// </summary>
   /// <param name="certificateName">The name of the certificate in Key Vault.</param>
   /// <param name="subjectNameValue">The certificate subject name<</param>
   /// <param name="signerCertificateName">The name of the signing certificate in Key Vault.</param>
   /// <param name="vaultUri">The URI of the Key Vault holding the certificates</param>
   /// <param name="tokenCredential">The authentication token provider.</param>
   /// <param name="expireDays">The number of days until the certificate expires.</param>
   public static async Task CreateSigningCertificateAsync(string certificateName, string subjectNameValue, string signerCertificateName, Uri vaultUri, TokenCredential tokenCredential, int expireMonth)
   {
      var client = new CertificateClient(vaultUri, tokenCredential);

      // Get the signer certificate and its associated keys
      (var signerName, var signerSignaturGenerator) = await KeyVaultGetSignerCertificateAsync(signerCertificateName, client, tokenCredential);

      var csr = await KeyVaultCreateSigningCertificateRequestAsync(certificateName, subjectNameValue, client, expireMonth);

      // Sign the CSR
      var cert = CertificateWorkerCore.SignCertificateRequest(csr, signerName, signerSignaturGenerator, expireMonth);

      // and upload it to Key Vault
      await CertificateWorkerCore.KeyVaultMergeCertificateAsync(certificateName, cert, client);
   }

   private static async Task<(X500DistinguishedName, X509SignatureGenerator)> KeyVaultGetSignerCertificateAsync(string certificateName, CertificateClient client, TokenCredential tokenCredential)
   {
      var cert = await client.GetCertificateAsync(certificateName);

      using var x509Cert = new X509Certificate2(cert.Value.Cer);
      var privateKeyId = cert.Value.KeyId;
      return (x509Cert.SubjectName, new KeyVaultX509SignatureGenerator(tokenCredential, privateKeyId, x509Cert.PublicKey));
   }

   private static async Task<CertificateRequest> KeyVaultCreateSigningCertificateRequestAsync(string certificateName, string subjectNameValue, CertificateClient client, int expireMonth)
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

      certSigningRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
      certSigningRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
      certSigningRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid(CodeSigningEnhancedKeyUsageOid, CodeSigningEnhancedKeyUsageOidFriendlyName)], true));

      return certSigningRequest;
   }
}
