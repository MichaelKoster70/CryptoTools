// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CommandLine;

namespace CertTools.AzureCertCore;

/// <summary>
/// Container class for the command line options common to all tools.
/// </summary>
public class OptionsBase
{
   /// <summary>
   /// Gets or set the x.509 Subject Name for the certificate.
   /// </summary>
   [Option("Subject", Required = true, HelpText = "The subject name for the certificate in the form \"CN=<subject name>\"")]
   public required string Subject { get; set; }

   /// <summary>
   /// Gets or sets the name of the certificate to create in Key Vault.
   /// </summary>
   [Option("CertificateName", Required = true, Group = "CertificateNameOrFile", HelpText = "The name of the certificate to create in Key Vault")]
   public required string CertificateName { get; set; }

   /// <summary>
   /// Gets or sets the Azure Key Vault URI where to upload the certificate.
   /// </summary>
   [Option("KeyVaultUri", Required = true, HelpText = "The URI to an Azure Key Vault")]
   public required string KeyVaultUri { get; set; }

   /// <summary>
   /// Gets or sets the Entra ID Tenant ID.
   /// </summary>
   [Option("TenantId", HelpText = "The Azure Entra ID Tenant ID to authenticate access to Key Vault")]
   public string? TenantId { get; set; }

   /// <summary>
   /// Gets or sets the Entra ID Application (Client) ID.
   /// </summary>
   [Option("ClientId", HelpText = "The Azure Entra ID Application (Client) ID of the application accessing Key Vault")]
   public string? ClientId { get; set; }

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
}
