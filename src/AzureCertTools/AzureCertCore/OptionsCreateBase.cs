// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CommandLine;

namespace CertTools.AzureCertCore;

/// <summary>
/// Container class for the command line options common to all tools creating certificates.
/// </summary>
public class OptionsCreateBase : OptionsBase
{
   /// <summary>Default key type: RSA software-backed.</summary>
   public const string DefaultKeyType = "Rsa";

   /// <summary>
   /// Gets or sets the key type for the certificate private key. Supported values: Ec, EcHsm, Rsa, RsaHsm. Default is Rsa.
   /// </summary>
   [Option("KeyType", Required = false, HelpText = "The key type for the certificate private key: Ec, EcHsm, Rsa, RsaHsm. Default is Rsa.")]
   public string KeyType { get; set; } = DefaultKeyType;

   /// <summary>
   /// Gets or sets a value indicating whether the private key is exportable from Azure Key Vault. Default is false.
   /// </summary>
   [Option("Exportable", Required = false, HelpText = "Whether the private key is exportable from Azure Key Vault. Default is false.")]
   public bool Exportable { get; set; } = false;

   /// <summary>
   /// Gets or sets the EC key curve name. Applicable only for Ec and EcHsm key types. Supported values: P256, P256K, P384, P521. Default is P384.
   /// </summary>
   [Option("KeyCurveName", Required = false, HelpText = "The EC key curve name (Ec and EcHsm only): P256, P256K, P384, P521. Default is P384.")]
   public string KeyCurveName { get; set; } = EcKeyCreationOptions.DefaultKeyCurveName;

   /// <summary>
   /// Gets or sets the RSA key size in bits. Applicable only for Rsa and RsaHsm key types. Supported values: 2048, 3072, 4096. Default is 4096.
   /// </summary>
   [Option("KeySize", Required = false, HelpText = "The RSA key size in bits (Rsa and RsaHsm only): 2048, 3072, 4096. Default is 4096.")]
   public int KeySize { get; set; } = RsaKeyCreationOptions.DefaultKeySize;

}
