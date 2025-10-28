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
internal class Options : OptionsBase
{
   /// <summary>
   /// Gets or sets the number of month until the certificate expires.
   /// </summary>
   [Option("ExpireMonths", Required = false, HelpText = "The number of months until the certificate expires, default if not specified is 240 months.")]
   public int ExpireMonths { get; set; } = 240;
}
