// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CertTools.AzureCertCore;
using CommandLine;

namespace CertTools.AzureCreateRootCert;

/// <summary>
/// Container class for the command line options.
/// </summary>
internal class Options : OptionsCreateBase
{
   /// <summary>
   /// Gets or set the x.509 Subject Name for the certificate.
   /// </summary>
   [Option("Subject", Required = true, HelpText = "The subject name for the certificate in the form \"CN=<subject name>\"")]
   public required string Subject { get; set; }

   /// <summary>
   /// Gets or sets the number of month until the certificate expires.
   /// </summary>
   [Option("ExpireMonths", Required = false, HelpText = "The number of months until the certificate expires, default if not specified is 240 months.")]
   public int ExpireMonths { get; set; } = 240;

   /// <summary>
   /// Gets or sets the path length constraint for the certificate. This is only relevant if the certificate is a CA certificate. If not specified, there will be no path length constraint.
   /// </summary>
   [Option("PathLengthConstraint", Required = false, HelpText = "The path length constraint for the certificate, default not present")]
   public int? PathLengthConstraint { get; set; } = null;
}
