// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertTools.CertCore;

namespace CertTools.CreateSigningCert;

/// <summary>
/// Class providing the functionality to create a code signing certificate.
/// </summary>
internal static class CertificateWorker
{
   /// <summary>RSA key size</summary>
   private const int RsaKeySize = 4096;

   /// <summary>
   /// Create a code signing certificate signed with the root loaded from CurrentUser\My stire
   /// </summary>
   /// <param name="subjectName">The certificate subject name</param>
   /// <param name="fileName">The name of the PFX file.</param>
   /// <param name="password">The password to protect the private key</param>
   /// <param name="signerThumbprint">Cert thumbprint identifying the root cert to load.</param>
   /// <param name="expireDays">The number of days until the certificate expires.</param>
   public static void CreateSigningCertificate(string subjectName, string fileName, string password, string signerThumbprint, int expireDays)
   {
      using var rootCert = LoadeSelfSignedRootCertificateFromStore(signerThumbprint);
      using var cert = CreateSigningCertificate(subjectName, rootCert, expireDays);

      // Export the certificate
      var certBytes = cert.Export(X509ContentType.Pfx, password);
      File.WriteAllBytes($"{fileName}.pfx", certBytes);
   }

   /// <summary>
   /// Create a code signing certificate signed with the root loaded from a PFX file
   /// </summary>
   /// <param name="subjectName">The certificate subject name</param>
   /// <param name="fileName">The name of the PFX file.</param>
   /// <param name="password">The password to protect the private key.</param>
   /// <param name="signerPfx">The PFX file holding the root cert.</param>
   /// <param name="signerPassword">The passwordprotec ting the root cert private key</param>
   /// <param name="expireDays">The number of days until the certificate expires.</param>
   public static void CreateSigningCertificate(string subjectName, string fileName, string password, string signerPfx, string signerPassword, int expireDays)
   {
      using var rootCert = LoadeSelfSignedRootCertificateFromFile(signerPfx, signerPassword);
      using var cert = CreateSigningCertificate(subjectName, rootCert, expireDays);

      // Export the certificate
      var certBytes = cert.Export(X509ContentType.Pfx, password);
      File.WriteAllBytes($"{fileName}.pfx", certBytes);
   }

   private static X509Certificate2 CreateSigningCertificate(string subjectName, X509Certificate2 rootCert, int expireDays)
   {
      var cspParameter = new CspParameters();
      cspParameter = new CspParameters(cspParameter.ProviderType, cspParameter.ProviderName, Guid.NewGuid().ToString());

      using var keyPair = new RSACryptoServiceProvider(RsaKeySize, cspParameter);

      // create a CSR
      var subject = new X500DistinguishedName(subjectName);
      var csr = new CertificateRequest(subject, keyPair, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);
      csr.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
      csr.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
      csr.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid(X509Constants.CodeSigningEnhancedKeyUsageOid, X509Constants.CodeSigningEnhancedKeyUsageOidFriendlyName)], true));

      // Create the Cert serial numbert
      byte[] serialNumber = new byte[9];
      using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
      {
         randomNumberGenerator.GetBytes(serialNumber);
      }

      // Create the certificate
      var utcNow = DateTimeOffset.UtcNow;
      using var cert = csr.Create(rootCert, utcNow.AddDays(-1), utcNow.AddDays(expireDays), serialNumber);

      return cert.CopyWithPrivateKey(keyPair);
   }

   private static X509Certificate2 LoadeSelfSignedRootCertificateFromStore(string certificateThumbprint)
   {
      X509Certificate2? root = null;

      try
      {
         using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
         store.Open(OpenFlags.ReadOnly);

         var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, true);

         if (certificates.Count > 0)
         {
            root = certificates[0];
            certificates.Remove(root);
         }
         else
         {
            certificates = store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, false);

            if (certificates.Count > 0)
            {
               root = certificates[0];
               certificates.Remove(root);
            }
         }

         DisposeCertificates(certificates);

         store.Close();
      }
      catch (CryptographicException)
      {
         // trown when the find type is invalid -> treat this as cert not present.
         root = null;
      }

      if (root == null)
      {
         throw new InvalidOperationException($"Root certificate with thumbprint {certificateThumbprint} not found");
      }

      return root;
   }

   private static X509Certificate2 LoadeSelfSignedRootCertificateFromFile(string fileName, string password)
   {
      return new X509Certificate2(fileName, password);
   }

   private static void DisposeCertificates(X509Certificate2Collection disposables)
   {
      foreach (var disposable in disposables)
      {
         try
         {
            disposable.Dispose();
         }
         catch (Exception)
         {
            //Ignore all failures
         }
      }
   }
}
