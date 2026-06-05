// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace CertTools.AzureCertCore;

/// <summary>
/// Container class for the key creation options controlling how the private key for a certificate is created in Azure Key Vault.
/// </summary>
public class KeyCreationOptions
{
   /// <summary>Default key type: RSA software-backed.</summary>
   public const string DefaultKeyType = "Rsa";

   /// <summary>Default EC key curve name.</summary>
   public const string DefaultKeyCurveName = "P384";

   /// <summary>Default RSA key size in bits.</summary>
   public const int DefaultKeySize = 4096;

   /// <summary>
   /// Gets or sets the key type. Supported values: Ec, EcHsm, Rsa, RsaHsm.
   /// </summary>
   public string KeyType { get; init; } = DefaultKeyType;

   /// <summary>
   /// Gets or sets a value indicating whether the private key is exportable from Azure Key Vault.
   /// </summary>
   public bool Exportable { get; init; } = false;

   /// <summary>
   /// Gets or sets the EC key curve name. Applicable only for EC and EcHsm key types. Supported values: P256, P256K, P384, P521.
   /// </summary>
   public string KeyCurveName { get; init; } = DefaultKeyCurveName;

   /// <summary>
   /// Gets or sets the RSA key size in bits. Applicable only for Rsa and RsaHsm key types. Supported values: 2048, 3072, 4096.
   /// </summary>
   public int KeySize { get; init; } = DefaultKeySize;
}
