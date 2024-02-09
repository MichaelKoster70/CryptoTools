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

namespace CertTools.AzureCreateRootCert;

/// <summary>
/// Class providing the functionality to create a self signed root certificate and upload it to Azure Key Vault.
/// </summary>
internal class CertificateWorker
{
   /// <summary>RSA key size</summary>
   private const int RsaKeySize = 4096;

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
       // create a self signed cert, Azure Key Vault does not support self signed CA certs
      using var cert = CreateSelfSignedRootCertificate(subjectNameValue);

      // Export with a generated password
      var password = GeneratePassword();
      var pfx = cert.Export(X509ContentType.Pkcs12, password);

      // Import the certificate into Azure Key Vault
      var certName = await AzureKeyVaultImportCertificateAsync(certificateName, pfx, password, vaultUri, tokenCredential);
      return certName;
   }

   private static async Task<string> AzureKeyVaultImportCertificateAsync(string certificateName, byte[] pfx, string password, Uri vaultUri, TokenCredential tokenCredential)
   {
      var client = new CertificateClient(vaultUri, tokenCredential);

      var importCertificateOptions = new ImportCertificateOptions(certificateName, pfx)
      {
         Password = password,
      };

      var certificateResponse = await client.ImportCertificateAsync(importCertificateOptions);

      return certificateResponse.Value.Name;
   }


   /// <summary>
   /// Create a self signed root certificate.
   /// </summary>
   /// <param name="subjectNameValue">The value (CN=<value>) to be used as Subject</param>
   /// <returns>The certtifc</returns>
   private static X509Certificate2 CreateSelfSignedRootCertificate(string subjectNameValue)
   {
      // Create a RSA keypair
      using var keyPair = RSA.Create(RsaKeySize);

      // create a CSR
      var subjectName = new X500DistinguishedName(subjectNameValue);
      var csr = new CertificateRequest(subjectName, keyPair, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);
      csr.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 0, true));
      csr.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.DigitalSignature, false));
      csr.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(csr.PublicKey, false));
      csr.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid(CodeSigningEnhancedKeyUsageOid, CodeSigningEnhancedKeyUsageOidFriendlyName)], false));

      // create a self signed cert
      var cert = csr.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));

      return cert;
   }

   private static string GeneratePassword()
   {
      // Generate a random password with 16 characters
      byte[] randomBytes = new byte[16];
      using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
      {
         rng.GetBytes(randomBytes);
      }

      return Convert.ToBase64String(randomBytes)[..16];
   }
}
