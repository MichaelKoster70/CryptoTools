// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CertTools.AzureCertCore;
using CommandLine;

namespace CertTools.AzureCreateSigningCert;

/// <summary>
/// Container class for the command line options.
/// </summary>
internal class Options : OptionsBase
{
   /// <summary>
   /// Gets or sets the name of the certificate PFX file to create.
   /// </summary>
   [Option("FileName", Required = true, Group = "CertificateNameOrFile", HelpText = "The absolute path for the PFX file to create.")]
   public required string? FileName { get; set; }

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
   [Option("ExpireMonth", Required = false, HelpText = "The number of month until the certificate expires, default if not specified is 1 month.")]
   public int ExpireMonth { get; set; } = 1;
}
