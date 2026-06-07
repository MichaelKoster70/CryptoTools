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
   /// <param name="reuseKey">Whether to reuse the key when renewing the certificate.</param>
   /// <param name="expireMonth">The expiry date in month from now.</param>
   /// <param name="keyOptions">The key creation options controlling the key type, size and exportability.</param>
   /// <returns>The <see cref="X509SignatureGenerator"/> based on the created certificate.</returns>
   public static async Task<X509SignatureGenerator> KeyVaultCreateSelfSignedSignatureGeneratorAsync(string certificateName, string subjectNameValue, CertificateClient client, TokenCredential tokenCredential, bool reuseKey, int expireMonth, KeyCreationOptions? keyOptions = null)
   {
      ArgumentNullException.ThrowIfNull(certificateName);
      ArgumentNullException.ThrowIfNull(subjectNameValue);
      ArgumentNullException.ThrowIfNull(client);
      ArgumentNullException.ThrowIfNull(tokenCredential);

      keyOptions ??= new RsaKeyCreationOptions();

      var certificatePolicy = CreateCertificatePolicy(WellKnownIssuerNames.Self, subjectNameValue, expireMonth, keyOptions, reuseKey);

      // Create the certificate, the operation will complete with the certificate
      var operation = await client.StartCreateCertificateAsync(certificateName, certificatePolicy);

      // Wait for the certificate to be created, get the public and signing key.
      _ = await operation.WaitForCompletionAsync();

      using var x509Cert = X509CertificateLoader.LoadCertificate(operation.Value.Cer);
      var privateKeyId = operation.Value.KeyId;

      return new KeyVaultX509SignatureGenerator(tokenCredential, privateKeyId, x509Cert.PublicKey);
   }

   /// <summary>
   /// Retrieves a certificate from Azure Key Vault and creates a signature generator for signing operations.
   /// </summary>
   /// <param name="certificateName">The name of the certificate to retrieve from Key Vault.</param>
   /// <param name="client">The <see cref="CertificateClient"/> instance used to interact with Azure Key Vault.</param>
   /// <param name="tokenCredential">The <see cref="TokenCredential"/> used to authenticate requests to Azure Key Vault.</param>
   /// <returns>A tuple containing the subject name of the certificate as an <see cref="X500DistinguishedName"/>  and an <see cref="X509SignatureGenerator"/> for performing cryptographic signing operations.</returns>
   public static async Task<(X500DistinguishedName, X509SignatureGenerator)> KeyVaultGetSignerCertificateAsync(string certificateName, CertificateClient client, TokenCredential tokenCredential)
   {
      ArgumentNullException.ThrowIfNull(certificateName);
      ArgumentNullException.ThrowIfNull(client);
      ArgumentNullException.ThrowIfNull(tokenCredential);

      var cert = await client.GetCertificateAsync(certificateName);

      using var x509Cert = X509CertificateLoader.LoadCertificate(cert.Value.Cer);
      var privateKeyId = cert.Value.KeyId;
      return (x509Cert.SubjectName, new KeyVaultX509SignatureGenerator(tokenCredential, privateKeyId, x509Cert.PublicKey));
   }

   /// <summary>
   /// Signs a certificate request and generates an X.509 certificate.
   /// </summary>
   /// <param name="csr">The certificate request to be signed. This parameter cannot be <see langword="null"/>.</param>
   /// <param name="signerName">The distinguished name of the certificate authority (CA) or signer. This parameter cannot be <see
   /// langword="null"/>.</param>
   /// <param name="signerSignature">The signature generator used to sign the certificate. This parameter cannot be <see langword="null"/>.</param>
   /// <param name="expireMonth">The validity period of the certificate, in months, starting from the current date.</param>
   /// <returns>An <see cref="X509Certificate2"/> object representing the signed certificate.</returns>
   public static X509Certificate2 SignCertificateRequest(CertificateRequest csr, X500DistinguishedName signerName, X509SignatureGenerator signerSignature, int expireMonth)
   {
      ArgumentNullException.ThrowIfNull(csr);
      ArgumentNullException.ThrowIfNull(signerName);
      ArgumentNullException.ThrowIfNull(signerSignature);

      // Create the Cert serial number
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
   /// Merges the given certificate into the existing certificate in Azure Key Vault.
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

   /// <summary>
   /// Creates an Azure Key Vault <see cref="CertificatePolicy"/> from the given options.
   /// </summary>
   /// <param name="issuerName">The issuer name (e.g. <see cref="WellKnownIssuerNames.Self"/> or <see cref="WellKnownIssuerNames.Unknown"/>).</param>
   /// <param name="subjectNameValue">The certificate subject name.</param>
   /// <param name="expireMonths">The validity period in months.</param>
   /// <param name="keyOptions">The key creation options.</param>
   /// <param name="reuseKey">Whether to reuse the existing key when renewing the certificate.</param>
   /// <returns>A configured <see cref="CertificatePolicy"/>.</returns>
   public static CertificatePolicy CreateCertificatePolicy(string issuerName, string subjectNameValue, int expireMonths, KeyCreationOptions keyOptions, bool reuseKey = true)
   {
      ArgumentNullException.ThrowIfNull(issuerName);
      ArgumentNullException.ThrowIfNull(subjectNameValue);
      ArgumentNullException.ThrowIfNull(keyOptions);

      var keyType = keyOptions.GetCertificateKeyType();

      var policy = new CertificatePolicy(issuerName, subjectNameValue)
      {
         KeyType = keyType,
         ReuseKey = reuseKey,
         Exportable = keyOptions.Exportable,
         ValidityInMonths = expireMonths,
      };

      if (keyOptions is EcKeyCreationOptions ecOptions)
      {
         policy.KeyCurveName = ecOptions.GetCertificateKeyCurveName();
      }
      else if (keyOptions is RsaKeyCreationOptions rsaOptions)
      {
         policy.KeySize = rsaOptions.KeySize;
      }

      return policy;
   }

   /// <summary>
   /// Determines the appropriate hash algorithm to use based on the provided key creation options. For EC keys, the hash algorithm is selected based on the curve name, while for RSA keys, a default hash algorithm is returned.
   /// </summary>
   /// <param name="keyOptions">The key creation options.</param>
   /// <returns>The appropriate hash algorithm.</returns>
   public static HashAlgorithmName GetHashAlgorithmName(KeyCreationOptions keyOptions)
   {
      ArgumentNullException.ThrowIfNull(keyOptions);

      return keyOptions switch
      {
         EcKeyCreationOptions ecOptions => ecOptions.KeyCurveName.ToUpperInvariant() switch
         {
            "P256" or "P256K" => HashAlgorithmName.SHA256,
            "P521" => HashAlgorithmName.SHA512,
            _ => HashAlgorithmName.SHA384,
         },
         _ => HashAlgorithmName.SHA384,
      };
   }

   public static RSASignaturePadding? GetRSASignaturePadding(KeyCreationOptions keyOptions)
   {
      ArgumentNullException.ThrowIfNull(keyOptions);

      return keyOptions switch
      {
         RsaKeyCreationOptions => RSASignaturePadding.Pkcs1,
         _ => null,
      };
   }
}
