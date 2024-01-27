// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CommandLine;

namespace CertTools.CreateRootCert;

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
   /// Gets or sets the path to the filename to export the certificate to.
   /// </summary>
   [Option("Name", Required = true, HelpText = "The name of the file to export the certificate to (PFX and CER)")]
   public required string Name { get; set; }

   /// <summary>
   /// Gets or sets the path to the PFX file to export the certificate to.
   /// </summary>
   [Option("Password", Required = false, HelpText = "The password to use for the PFX file")]
   public string? Password { get; set; }
}
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
