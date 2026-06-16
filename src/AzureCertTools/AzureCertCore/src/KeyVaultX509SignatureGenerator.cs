// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace CertTools.AzureCertCore;

/// <summary>
/// The X509 signature generator to sign a digest with a KeyVault key.
/// Supports both RSA (PKCS#1) and EC (ECDSA) keys stored in Azure Key Vault.
/// </summary>
/// <param name="credential">The credentials to use to authenticate against KeyVault.</param>
/// <param name="signingKey">The KeyVault signing key</param>
/// <param name="signingPublicKey">The public key of the signing certificate.</param>
public class KeyVaultX509SignatureGenerator(TokenCredential credential, Uri signingKey, PublicKey signingPublicKey) : X509SignatureGenerator
{
   // Key algorithm OIDs
   private const string EcKeyOid = "1.2.840.10045.2.1";

   // EC curve OIDs
   private const string P256CurveOid = "1.2.840.10045.3.1.7";
   private const string P256KCurveOid = "1.3.132.0.10";
   private const string P384CurveOid = "1.3.132.0.34";
   private const string P521CurveOid = "1.3.132.0.35";

   private readonly TokenCredential credential = credential;
   private readonly Uri signingKey = signingKey;
   private readonly PublicKey signingPublicKey = signingPublicKey;

   /// <summary>Gets a value indicating whether the signing key is an EC key.</summary>
   private bool IsEcKey => signingPublicKey.Oid.Value == EcKeyOid;

   /// <summary>
   /// Produces a signature for the specified data using the specified hash algorithm and encodes the results appropriately for X.509 signature values.
   /// </summary>
   /// <param name="data">The input data for which to produce the signature.</param>
   /// <param name="hashAlgorithm">The hash algorithm to use to produce the signature.</param>
   /// <returns>The X.509 signature for the specified data.</returns>
   public override byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm)
   {
      if (IsEcKey)
      {
         hashAlgorithm = GetHashAlgorithmForEcKey();
      }

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

         if (IsEcKey)
         {
            var rawSignature = KeyVaultSignEcDigestAsync(signingKey, digest, hashAlgorithm).GetAwaiter().GetResult();
            return ConvertEcdsaSignatureToDer(rawSignature);
         }
         else
         {
            return KeyVaultSignDigestAsync(signingKey, digest, hashAlgorithm, RSASignaturePadding.Pkcs1).GetAwaiter().GetResult();
         }
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
      if (IsEcKey)
      {
         // For EC keys, the hash algorithm is determined by the curve, not by the caller
         hashAlgorithm = GetHashAlgorithmForEcKey();

         // ecdsaWithSHA256 OID 1.2.840.10045.4.3.2 – no parameters
         // ecdsaWithSHA384 OID 1.2.840.10045.4.3.3 – no parameters
         // ecdsaWithSHA512 OID 1.2.840.10045.4.3.4 – no parameters
         if (hashAlgorithm == HashAlgorithmName.SHA256)
         {
            return [48, 10, 6, 8, 42, 134, 72, 206, 61, 4, 3, 2];
         }
         else if (hashAlgorithm == HashAlgorithmName.SHA384)
         {
            return [48, 10, 6, 8, 42, 134, 72, 206, 61, 4, 3, 3];
         }
         else if (hashAlgorithm == HashAlgorithmName.SHA512)
         {
            return [48, 10, 6, 8, 42, 134, 72, 206, 61, 4, 3, 4];
         }

         throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), $"The hash algorithm {hashAlgorithm.Name} is not supported for EC keys.");
      }

      // RSA PKCS#1 algorithm identifiers (with NULL parameters)
      if (hashAlgorithm == HashAlgorithmName.SHA256)
      {
         // RsaPkcs1Sha256 = "1.2.840.113549.1.1.11";
         return [48, 13, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 11, 5, 0];
      }
      else if (hashAlgorithm == HashAlgorithmName.SHA384)
      {
         // RsaPkcs1Sha384 = "1.2.840.113549.1.1.12";
         return [48, 13, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 12, 5, 0];
      }
      else if (hashAlgorithm == HashAlgorithmName.SHA512)
      {
         // RsaPkcs1Sha512 = "1.2.840.113549.1.1.13";
         return [48, 13, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 13, 5, 0];
      }
      else
      {
         throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), $"The hash algorithm {hashAlgorithm.Name} is not supported.");
      }
   }

   /// <summary>Returns the hash algorithm appropriate for the EC key's curve.</summary>
   private HashAlgorithmName GetHashAlgorithmForEcKey()
   {
      var curveOid = GetEcCurveOid();
      return curveOid switch
      {
         P256CurveOid or P256KCurveOid => HashAlgorithmName.SHA256,
         P384CurveOid => HashAlgorithmName.SHA384,
         P521CurveOid => HashAlgorithmName.SHA512,
         _ => HashAlgorithmName.SHA384   // unknown curves default to SHA-384
      };
   }

   /// <summary>Reads the EC curve OID from the signing public key's encoded parameters.</summary>
   private string? GetEcCurveOid()
   {
      var encoded = signingPublicKey.EncodedParameters?.RawData;
      if (encoded == null)
      {
         return null;
      }

      try
      {
         var reader = new AsnReader(encoded, AsnEncodingRules.DER);
         return reader.ReadObjectIdentifier();
      }
      catch (AsnContentException)
      {
         // QUICKFIX:
         // If the parameters are not in the expected format, we won't be able to determine the curve OID.
         // If we land here, we assume it's a P-256K curve, which is the only non-standard curve currently supported by Azure KeyVault, and return its OID directly.
         // This allows us to continue functioning even if the parameters are not in the expected format.
         return P256KCurveOid;
      }
   }

   private async Task<byte[]> KeyVaultSignEcDigestAsync(Uri keyUri, byte[] digest, HashAlgorithmName hashAlgorithm)
   {
      SignatureAlgorithm algorithm;
      var curveOid = GetEcCurveOid();

      // P-256K requires ES256K in Key Vault
      if (curveOid == "1.3.132.0.10")
      {
         algorithm = new SignatureAlgorithm("ES256K");
      }
      else if (hashAlgorithm == HashAlgorithmName.SHA256)
      {
         algorithm = SignatureAlgorithm.ES256;
      }
      else if (hashAlgorithm == HashAlgorithmName.SHA384)
      {
         algorithm = SignatureAlgorithm.ES384;
      }
      else if (hashAlgorithm == HashAlgorithmName.SHA512)
      {
         algorithm = SignatureAlgorithm.ES512;
      }
      else
      {
         throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), $"The hash algorithm {hashAlgorithm.Name} is not supported for EC signing.");
      }

      var cryptoClient = new CryptographyClient(keyUri, credential);
      SignResult result = await cryptoClient.SignAsync(algorithm, digest);
      return result.Signature;
   }

   /// <summary>
   /// Converts an ECDSA signature from the IEEE-P1363 raw r||s format (as returned by Azure Key Vault)
   /// to the DER-encoded ASN.1 SEQUENCE { INTEGER r, INTEGER s } format required by X.509.
   /// </summary>
   private static byte[] ConvertEcdsaSignatureToDer(byte[] rawSignature)
   {
      if (rawSignature.Length == 0 || rawSignature.Length % 2 != 0)
      {
         throw new CryptographicException("ECDSA signature must be a non-empty, even-length r||s byte array.");
      }

      int halfLen = rawSignature.Length / 2;

      var writer = new AsnWriter(AsnEncodingRules.DER);
      using (writer.PushSequence())
      {
         // WriteInteger treats the span as an unsigned big-endian integer and adds
         // a leading 0x00 byte if the high bit is set, which is correct DER encoding.
         WriteUnsignedInteger(writer, rawSignature.AsSpan(0, halfLen));
         WriteUnsignedInteger(writer, rawSignature.AsSpan(halfLen));
      }

      return writer.Encode();
   }


   private static void WriteUnsignedInteger(AsnWriter writer, ReadOnlySpan<byte> value)
   {
      // Trim leading zeros
      while (!value.IsEmpty && value[0] == 0x00)
      {
         value = value[1..];
      }

      // DER INTEGER zero value
      if (value.IsEmpty)
      {
         writer.WriteInteger(0);
         return;
      }

      // Write as unsigned big-endian integer
      writer.WriteIntegerUnsigned(value);
   }


   private async Task<byte[]> KeyVaultSignDigestAsync(Uri keyUri, byte[] digest, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
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
            throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), $"The hash algorithm {hashAlgorithm.Name} is not supported.");
         }
      }
      else
      {
         throw new ArgumentOutOfRangeException(nameof(padding), $"The padding algorithm {padding} is not supported.");
      }

      // create a client for performing cryptographic operations on Key Vault
      var cryptoClient = new CryptographyClient(keyUri, credential);

      SignResult result = await cryptoClient.SignAsync(algorithm, digest);

      return result.Signature;
   }
}
