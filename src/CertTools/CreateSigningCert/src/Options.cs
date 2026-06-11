// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CertTools.CertCore;
using CommandLine;

namespace CertTools.CreateSigningCert;

/// <summary>
/// Container class for the command line options.
/// </summary>
internal class Options : OptionsBase
{
   /// <summary>
   /// Gets the certificate thumbprint of the certificate to use for signing.
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
   [Option("ExpireMonths", Required = false, HelpText = "The number of months until the certificate expires, default if not specified is 12 months.")]
   public int ExpireMonths { get; set; } = 12;
}
