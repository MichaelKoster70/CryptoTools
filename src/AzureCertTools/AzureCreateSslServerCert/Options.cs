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
internal class Options : OptionsCreateBase
{
   /// <summary>
   /// Gets or set the x.509 Subject Name for the certificate.
   /// </summary>
   [Option("FQDN", Required = true, HelpText = "The fully qualified DNS domain name")]
   public required string FQDN { get; set; }

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
}
