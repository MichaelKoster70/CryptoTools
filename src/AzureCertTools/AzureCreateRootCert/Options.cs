// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CommandLine;

namespace CertTools.AzureCreateRootCert;

/// <summary>
/// Container class for the command line options.
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal class Options
{
   /// <summary>
   /// Gets or set the x.509 Subject Name for the certificate.
   /// </summary>
   [Option("Subject", Required = true, HelpText = "The subject name for the certificate in the form \"CN=<subject name>\"")]
   public required string Subject { get; set; }

   /// <summary>
   /// Gets or sets the name of the certificate to create in Key Vault.
   /// </summary>
   [Option("Name", Required = true, HelpText = "The name of the certificate to create in Key Vault")]
   public required string Name { get; set; }

   /// <summary>
   /// Gets or sets the Azure Key Vault URI where to upload the certifcate.
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
   /// Gets or sets the interactive login flag.
   /// </summary>
   [Option("ClientSecret", Required = true, SetName="ClientSecret", HelpText = "The Azure Entra ID Application (Client) Secret of the application accessing Key Vault")]
   public required string ClientSecret { get; set; }

}
#pragma warning restore CA1812 // Avoid uninstantiated internal classes

//DGU8Q ~ylNou.IaJCrmp9sAtdyokTYC6eqeiZNbNI