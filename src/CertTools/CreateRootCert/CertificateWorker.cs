// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertTools.CreateRootCert;

/// <summary>
/// Class providing the functionality to create a self signed root certificate.
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
   /// <param name="subjectNameValue">The subject name for the certificate.</param>
   /// <param name="fileName">The filename w/o extension of the cert files.</param>
   /// <param name="passwordValue">The password protecting the private key.</param>
   /// <paramref name="expireMonth"/>The number of months the certificate is valid.</param>
   public static string CreateRootCert(string subjectNameValue, string fileName, string passwordValue, int expireMonth)
   {
      // Create the certificate and export as PFX and CER
      var cert = CreateSelfSignedRootCertificate(subjectNameValue, expireMonth);
      using (cert = StoreSelfSignedRootCertificate(cert))
      {
         // export the certificate as PFX and CER
         var pfx = cert.Export(X509ContentType.Pfx, passwordValue);
         var cer = cert.Export(X509ContentType.Cert);

         // write the files
         File.WriteAllBytes($"{fileName}.pfx", pfx);
         File.WriteAllBytes($"{fileName}.cer", cer);

         return cert.Thumbprint;
      }
   }

   /// <summary>
   /// Create a self signed root certificate.
   /// </summary>
   /// <param name="subjectNameValue">The value (CN=<value>) to be used as Subject.</param>
   /// <paramref name="expireMonth"/>The number of months the certificate is valid.</param>
   /// <returns>The certtifc</returns>
   private static X509Certificate2 CreateSelfSignedRootCertificate(string subjectNameValue, int expireMonth)
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
      var cert = csr.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddMonths(expireMonth));

      return cert;
   }

   /// <summary>
   /// Store the self signed root certificate in the CurrentUser\My store.
   /// </summary>
   /// <param name="certificate">The certificate to store.</param>
   /// <returns>The cert instance with public and private keys.</returns>
   private static X509Certificate2 StoreSelfSignedRootCertificate(X509Certificate2 certificate)
   {
      // a newly created certificate is not exportable, so we need to export and re-import it
      var export = certificate.Export(X509ContentType.Pkcs12, "");
      var friendlyName = certificate.FriendlyName;
      certificate.Dispose();
      certificate = new X509Certificate2(export, string.Empty, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable)
      {
         FriendlyName = friendlyName
      };

      Array.Clear(export, 0, export.Length);

      using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
      {
         store.Open(OpenFlags.ReadWrite);
         store.Add(certificate);
         store.Close();
      }

      return certificate;
   }
}
