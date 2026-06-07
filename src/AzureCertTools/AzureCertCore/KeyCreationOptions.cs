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
   /// <summary>
   /// Gets or sets the RSA key size in bits. Supported values: 2048, 3072, 4096.
   /// </summary>
   public int KeySize { get; init; } = 4096;
}

/// <summary>
/// Enumeration of supported EC key curves for EC key creation in Azure Key Vault.
/// </summary>
public enum EcKeyCurve
{
   P256,
   P256K,
   P384,
   P521
}

/// <summary>
/// Container class for the EC key creation options controlling how the EC private key for a certificate is created in Azure Key Vault.
/// </summary>
public class EcKeyCreationOptions : KeyCreationOptions
{
   /// <summary>
   /// Gets or sets the EC key curve
   /// </summary>
   public EcKeyCurve KeyCurve { get; init; } = EcKeyCurve.P384;
}