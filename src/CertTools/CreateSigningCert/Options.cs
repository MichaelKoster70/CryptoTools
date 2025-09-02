// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CommandLine;

namespace CertTools.CreateSigningCert;

/// <summary>
/// Container class for the command line options.
/// </summary>
internal class Options
{
   /// <summary>
   /// Gets or set the x.509 Subject Name for the certificate.
   /// </summary>
   [Option("Subject", Required = true, HelpText = "The subject name for the certificate in the form \"CN=<subject name>\"")]
   public required string Subject { get; set; }

   /// <summary>
   /// Gets or sets the path to the filename to export the certificate to.
   /// </summary>
   [Option("Name", Required = true, HelpText = "The name of the file to export the certificate to (PFX and CER)")]
   public required string Name { get; set; }

   /// <summary>
   /// Gets or sets the path to the PFX file to export the certificate to.
   /// </summary>
   [Option("Password", Required = false, HelpText = "The password to use for the PFX file")]
   public string? Password { get; set; }

   /// <summary>
   /// Gets the cerificate thumbprint of the certificate to use for signing.
   /// </summary>
   [Option("SignerThumbprint", SetName = "certStore", Required = true, HelpText = "The thumbprint of the signing certificate looked up in CurrentUser\\My store")]
   public required string SignerThumbprint { get; set; }

   /// <summary>
   /// Gets the PFX file holding the certificate to use for signing.
   /// </summary>
   [Option("SignerPfx", SetName = "pfx", Required = true, HelpText = "The PFX file of the signing certificate looked up in CurrentUser\\My store")]
   public required string SignerPfx { get; set; }

   /// <summary>
   /// Gets the password to unlock the private key as part of the PFX file 
   /// </summary>
   [Option("SignerPassword", Required = false, HelpText = "The password to use for the signer certificate")]
   public string? SignerPassword { get; set; }

   /// <summary>
   /// Gets or sets the number of days until the certificate expires.
   /// </summary>
   [Option("ExpireDays", Required = false, HelpText = "The number of days until the certificate expires, default if not specifed is 365 days.")]
   public int ExpireDays { get; set; } = 365;
}
