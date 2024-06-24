// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using System.Runtime.ConstrainedExecution;

namespace CertTools.AzureCertCore;

/// <summary>
/// Class providing the shared functionality to create x.509 certificates in Azure Key Vault.
/// </summary>
public static class CertificateWorkerCore
{
   /// <summary>RSA key size in bits</summary>
   public const int RsaKeySize = 4096;

   /// <summary>
   /// Create a self signed certificate with the given subject name in Azure Key Vault.
   /// </summary>
   /// <param name="certificateName">The Key Vault Certificate name.</param>
   /// <param name="subjectNameValue">The certificate subject name</param>
   /// <param name="client">The client representing the Azure Key Vault instance to hold the certificate.</param>
   /// <param name="tokenCredential">The authentication token provider.</param>
   /// <param name="expireMonth">The expiry date in month from now.</param>
   /// <returns>The signanture generator based on the created certificate.</returns>
   public static async Task<X509SignatureGenerator> KeyVaultCreateSelfSignedCertAsync(string certificateName, string subjectNameValue, CertificateClient client, TokenCredential tokenCredential, bool reuseKey, int expireMonth)
   {
      ArgumentNullException.ThrowIfNull(certificateName);
      ArgumentNullException.ThrowIfNull(subjectNameValue);
      ArgumentNullException.ThrowIfNull(client);
      ArgumentNullException.ThrowIfNull(tokenCredential);

      var certificatePolicy = new CertificatePolicy(WellKnownIssuerNames.Self, subjectNameValue)
      {
         KeyType = CertificateKeyType.Rsa,
         KeySize = RsaKeySize,
         ReuseKey = reuseKey,
         Exportable = false,
         ValidityInMonths = expireMonth
      };

      // Create the certificate, the operation will complete with the certificate
      var operation = await client.StartCreateCertificateAsync(certificateName, certificatePolicy);

      // Wait for the certificate to be created, get the public and signing key.
      _= await operation.WaitForCompletionAsync();

      using var x509Cert = new X509Certificate2(operation.Value.Cer);
      var privateKeyId = operation.Value.KeyId;

      return new KeyVaultX509SignatureGenerator(tokenCredential, privateKeyId, x509Cert.PublicKey);
   }

   public static X509Certificate2 SignCertificateRequest(CertificateRequest csr, X500DistinguishedName signerName, X509SignatureGenerator signerSignature, int expireMonth)
   {
      ArgumentNullException.ThrowIfNull(csr);
      ArgumentNullException.ThrowIfNull(signerName);
      ArgumentNullException.ThrowIfNull(signerSignature);

      // Create the Cert serial numbert
      byte[] serialNumber = new byte[9];
      using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
      {
         randomNumberGenerator.GetBytes(serialNumber);
      }

      // Create the certificate
      var utcNow = DateTimeOffset.UtcNow;
      return csr.Create(signerName, signerSignature, utcNow.AddDays(-1), utcNow.AddMonths(expireMonth), serialNumber);
   }

   /// <summary>
   /// Merges the given certificate into the existing certificater in Azure Key Vault.
   /// </summary>
   /// <param name="certificateName">The certificate name to merge.</param>
   /// <param name="certificate">The .NEt certificate source.</param>
   /// <param name="client">The client representing the Azure Key Vault holding the cert.</param>
   public static async Task KeyVaultMergeCertificateAsync(string certificateName, X509Certificate2 certificate, CertificateClient client)
   {
      ArgumentNullException.ThrowIfNull(certificateName);
      ArgumentNullException.ThrowIfNull(certificate);
      ArgumentNullException.ThrowIfNull(client);

      var x509Certificate = certificate.Export(X509ContentType.Cert);
      var operation = new MergeCertificateOptions(certificateName, [x509Certificate]);
      _ = await client.MergeCertificateAsync(operation);
   }
}
