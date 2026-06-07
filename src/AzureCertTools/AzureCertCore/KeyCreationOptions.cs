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
public abstract class KeyCreationOptions
{
   /// <summary>
   /// Gets or sets a value indicating whether the private key is exportable from Azure Key Vault.
   /// </summary>
   public bool Exportable { get; init; } = false;

   /// <summary>
   /// Gets or sets a value indicating whether the private key should be created in an HSM-protected key vault storage.
   /// </summary>
   public bool HsmBacked { get; init; } = false;
}

/// <summary>
/// Container class for the RSA key creation options controlling how the RSA private key for a certificate is created in Azure Key Vault.
/// </summary>
public class RsaKeyCreationOptions : KeyCreationOptions
{
   /// <summary>Default RSA key size in bits.</summary>
   public const int DefaultKeySize = 4096;

   /// <summary>
   /// Gets or sets the RSA key size in bits. Supported values: 2048, 3072, 4096.
   /// </summary>
   public int KeySize { get; init; } = DefaultKeySize;
}

/// <summary>
/// Container class for the EC key creation options controlling how the EC private key for a certificate is created in Azure Key Vault.
/// </summary>
public class EcKeyCreationOptions : KeyCreationOptions
{
   /// <summary>Default EC key curve name.</summary>
   public const string DefaultKeyCurveName = "P384";

   /// <summary>
   /// Gets or sets the EC key curve name. Supported values: P256, P256K, P384, P521.
   /// </summary>
   public string KeyCurveName { get; init; } = DefaultKeyCurveName;
}