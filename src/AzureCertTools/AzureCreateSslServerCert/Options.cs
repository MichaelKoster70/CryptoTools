// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CertTools.AzureCertCore;
using CommandLine;

namespace CertTools.AzureCreateSslServerCert;

/// <summary>
/// Container class for the command line options.
/// </summary>
internal class Options
{
   /// <summary>
   /// Gets or set the x.509 Subject Name for the certificate.
   /// </summary>
   [Option("FQDN", Required = true, HelpText = "The fully qualified DNS domain name")]
   public required string FQDN { get; set; }

   /// <summary>
   /// Gets or sets the name of the certificate to create in Key Vault.
   /// </summary>
   [Option("CertificateName", Required = true, HelpText = "The name of the file to export the certificate to (PFX and CER) / Azure KeyVault Certificate Name")]
   public required string CertificateName { get; set; }

   /// <summary>
   /// Gets or sets the the flag on whether to create the certificate locally or on KeyVault
   /// </summary>
   [Option("Local", Required = false, HelpText = "Flag to create the certificate locally (true) or in Key Vault (false)")]
   public required bool Local { get; set; } = false;

   /// <summary>
   /// Gets or sets the path to the PFX file to export the certificate to.
   /// </summary>
   [Option("Password", Required = false, HelpText = "The password to use for the PFX file")]
   public string? Password { get; set; }

   /// <summary>
   /// Gets or sets the name of the signer certificate stored in Key Vault.
   /// </summary>
   [Option("SignerCertificateName", Required = true, HelpText = "The name of the signer certificate in Key Vault")]
   public required string SignerCertificateName { get; set; }

   /// <summary>
   /// Gets or sets the number of month until the certificate expires.
   /// </summary>
   [Option("ExpireMonth", Required = false, HelpText = "The number of month until the certificate expires, default if not specifed is 1 month.")]
   public int ExpireMonth { get; set; } = 1;

   /// <summary>
   /// Gets or sets the Azure Key Vault URI where to upload the certificate.
   /// </summary>
   [Option("KeyVaultUri", Required = true, HelpText = "The URI to an Azure Key Vault")]
   public required string KeyVaultUri { get; set; }

   /// <summary>
   /// Gets or sets the Entra ID Tenant ID.
   /// </summary>
   [Option("TenantId", Required = true, HelpText = "The Azure Entra ID Tenant ID to authenticate access to Key Vault")]
   public required string TenantId { get; set; }

   /// <summary>
   /// Gets or sets the Entra ID Application (Client) ID.
   /// </summary>
   [Option("ClientId", Required = true, HelpText = "The Azure Entra ID Application (Client) ID of the application accessing Key Vault")]
   public required string ClientId { get; set; }

   /// <summary>
   /// Gets or sets the interactive login flag.
   /// </summary>
   [Option("Interactive", Required = true, SetName="Interactive", HelpText = "Use interactive login")]
   public required bool Interactive { get; set; }

   /// <summary>
   /// Gets or sets the Entra ID Application (Client) Secret.
   /// </summary>
   [Option("ClientSecret", Required = true, SetName="ClientSecret", HelpText = "The Azure Entra ID Application (Client) Secret of the application accessing Key Vault")]
   public required string ClientSecret { get; set; }

   /// <summary>
   /// Gets or sets the managed identity login flag.
   /// </summary>
   [Option("WorkloadIdentity", Required = true, SetName = "WorkloadIdentity", HelpText = "Use Workload Identity to access Key Vault")]
   public required bool WorkloadIdentity { get; set; }

   /// <summary>
   /// Gets or sets the key type for the certificate private key. Supported values: Ec, EcHsm, Rsa, RsaHsm. Default is Rsa.
   /// </summary>
   [Option("KeyType", Required = false, HelpText = "The key type for the certificate private key: Ec, EcHsm, Rsa, RsaHsm. Default is Rsa.")]
   public string KeyType { get; set; } = KeyCreationOptions.DefaultKeyType;

   /// <summary>
   /// Gets or sets a value indicating whether the private key is exportable from Azure Key Vault. Default is false.
   /// </summary>
   [Option("Exportable", Required = false, HelpText = "Whether the private key is exportable from Azure Key Vault. Default is false.")]
   public bool Exportable { get; set; } = false;

   /// <summary>
   /// Gets or sets the EC key curve name. Applicable only for Ec and EcHsm key types. Supported values: P256, P256K, P384, P521. Default is P384.
   /// </summary>
   [Option("KeyCurveName", Required = false, HelpText = "The EC key curve name (Ec and EcHsm only): P256, P256K, P384, P521. Default is P384.")]
   public string KeyCurveName { get; set; } = KeyCreationOptions.DefaultKeyCurveName;

   /// <summary>
   /// Gets or sets the RSA key size in bits. Applicable only for Rsa and RsaHsm key types. Supported values: 2048, 3072, 4096. Default is 4096.
   /// </summary>
   [Option("KeySize", Required = false, HelpText = "The RSA key size in bits (Rsa and RsaHsm only): 2048, 3072, 4096. Default is 4096.")]
   public int KeySize { get; set; } = KeyCreationOptions.DefaultKeySize;
}
