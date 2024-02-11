// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace CertTools.AzureCreateSigningCert;

/// <summary>
/// The X509 signature generator to sign a digest with a KeyVault key.
/// </summary>
/// <param name="credential">The credetials to use to authenticate against KeyVault.</param>
/// <param name="signingKey">The KeyVault signing key</param>
/// <param name="issuerCertificate">The issuer certificate used for signing</param>
internal class KeyVaultX509SignatureGenerator(TokenCredential credential, Uri signingKey, PublicKey signingPublicKey) : X509SignatureGenerator
{
   private readonly TokenCredential credential = credential;
   private readonly Uri signingKey = signingKey;
   private readonly PublicKey signingPublicKey = signingPublicKey;

   /// <summary>
   /// Produces a signature for the specified data using the specified hash algorithm and encodes the results appropriately for X.509 signature values.
   /// </summary>
   /// <param name="data">The input data for which to produce the signature.</param>
   /// <param name="hashAlgorithm">The hash algorithm to use to produce the signature.</param>
   /// <returns>The X.509 signature for the specified data.</returns>
   public override byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm)
   {
      HashAlgorithm hash;
      if (hashAlgorithm == HashAlgorithmName.SHA256)
      {
         hash = SHA256.Create();
      }
      else if (hashAlgorithm == HashAlgorithmName.SHA384)
      {
         hash = SHA384.Create();
      }
      else if (hashAlgorithm == HashAlgorithmName.SHA512)
      {
         hash = SHA512.Create();
      }
      else
      {
         throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), $"The hash algorithm {hashAlgorithm.Name} is not supported.");
      }

      using (hash)
      {
         var digest = hash.ComputeHash(data);
         var signature = KeyVaultSignDigestAsync(signingKey, digest, hashAlgorithm, RSASignaturePadding.Pkcs1).GetAwaiter().GetResult();
         return signature;
      }
   }


   /// <summary>
   /// Produces the certificate's public key that has the correctly encoded Oid, public key parameters and public key values.
   /// </summary>
   /// <returns>The certificate's public key.</returns>
   protected override PublicKey BuildPublicKey() => signingPublicKey;

   /// <summary>
   /// Encodes the X.509 algorithm identifier for this signature.
   /// </summary>
   /// <param name="hashAlgorithm">The hash algorithm to use for encoding.</param>
   /// <returns>The encoded value for the X.509 algorithm identifier.</returns>
   /// <exception cref="ArgumentOutOfRangeException"></exception>
   public override byte[] GetSignatureAlgorithmIdentifier(HashAlgorithmName hashAlgorithm)
   {
      byte[] oidSequence;

      if (hashAlgorithm == HashAlgorithmName.SHA256)
      {
         //const string RsaPkcs1Sha256 = "1.2.840.113549.1.1.11";
         oidSequence = [48, 13, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 11, 5, 0];
      }
      else if (hashAlgorithm == HashAlgorithmName.SHA384)
      {
         //const string RsaPkcs1Sha384 = "1.2.840.113549.1.1.12";
         oidSequence = [48, 13, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 12, 5, 0];
      }
      else if (hashAlgorithm == HashAlgorithmName.SHA512)
      {
         //const string RsaPkcs1Sha512 = "1.2.840.113549.1.1.13";
         oidSequence = [48, 13, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 13, 5, 0];
      }
      else
      {
         throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), $"The hash algorithm {hashAlgorithm.Name} is not supported.");
      }

      return oidSequence;
   }

   private async Task<byte[]> KeyVaultSignDigestAsync(Uri signingKey, byte[] digest, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
   {
      SignatureAlgorithm algorithm;

      if (padding == RSASignaturePadding.Pkcs1)
      {
         if (hashAlgorithm == HashAlgorithmName.SHA256)
         {
            algorithm = SignatureAlgorithm.RS256;
         }
         else if (hashAlgorithm == HashAlgorithmName.SHA384)
         {
            algorithm = SignatureAlgorithm.RS384;
         }
         else if (hashAlgorithm == HashAlgorithmName.SHA512)
         {
            algorithm = SignatureAlgorithm.RS512;
         }
         else
         {
            throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), $"The hash algorithm {signingKey} is not supported.");
         }
      }
      else
      {
         throw new ArgumentOutOfRangeException(nameof(padding), $"The padding algorithm {padding} is not supported.");
      }

      // create a client for performing cryptographic operations on Key Vault
      var cryptoClient = new CryptographyClient(signingKey, credential);

      SignResult result = await cryptoClient.SignAsync(algorithm, digest);

      return result.Signature;
   }
}
