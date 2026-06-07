// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography;
using Azure.Security.KeyVault.Certificates;

namespace CertTools.AzureCertCore;

public static class KeyCreationOptionsExtensions
{
   /// <summary>
   /// Gets the <see cref="CertificateKeyType"/> for the certificate based on the type of the <see cref="KeyCreationOptions"/> instance.
   /// </summary>
   /// <param name="options">The key creation options.</param>
   /// <returns>The corresponding <see cref="CertificateKeyType"/>.</returns>
   public static CertificateKeyType GetCertificateKeyType(this KeyCreationOptions options)
   {
      ArgumentNullException.ThrowIfNull(options);

      return options switch
      {
         RsaKeyCreationOptions opts => opts.HsmBacked ? CertificateKeyType.RsaHsm : CertificateKeyType.Rsa,
         EcKeyCreationOptions opts => opts.HsmBacked ? CertificateKeyType.EcHsm : CertificateKeyType.Ec,
         _ => throw new NotSupportedException($"Unsupported key creation options type '{options.GetType().FullName}'.")
      };
   }

   /// <summary>
   /// Gets the <see cref="CertificateKeyCurveName"/> for the EC key based on the <see cref="EcKeyCreationOptions.KeyCurveName"/> property.
   /// </summary>
   /// <param name="options">The EC key creation options.</param>
   public static CertificateKeyCurveName GetCertificateKeyCurveName(this EcKeyCreationOptions options)
   {
      ArgumentNullException.ThrowIfNull(options);

      return options.KeyCurveName.ToUpperInvariant() switch
      {
         "P256" => CertificateKeyCurveName.P256,
         "P256K" => CertificateKeyCurveName.P256K,
         "P384" => CertificateKeyCurveName.P384,
         "P521" => CertificateKeyCurveName.P521,
         _ => throw new NotSupportedException($"Unsupported EC key curve name '{options.KeyCurveName}'. Supported values are: P256, P256K, P384, P521.")
      };
   }

   /// <summary>
   /// Gets the <see cref="ECCurve"/> for the EC key based on the <see cref="EcKeyCreationOptions.KeyCurveName"/> property.
   /// </summary>
   /// <param name="options">The EC key creation options.</param>
   public static ECCurve GetEcCurve(this EcKeyCreationOptions options)
   {
      ArgumentNullException.ThrowIfNull(options);

      return options.KeyCurveName.ToUpperInvariant() switch
      {
         "P256" => ECCurve.NamedCurves.nistP256,
         "P256K" => ECCurve.CreateFromFriendlyName("secp256k1"),
         "P384" => ECCurve.NamedCurves.nistP384,
         "P521" => ECCurve.NamedCurves.nistP521,
         _ => throw new NotSupportedException($"Unsupported EC curve '{options.KeyCurveName}'.")
      };
   }
}
