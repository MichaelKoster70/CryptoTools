// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CertTools.CertCore;
using CommandLine;

namespace CertTools.CreateRootCert;

/// <summary>
/// Container class for the command line options.
/// </summary>
internal class Options : OptionsBase  
{
   /// <summary>
   /// Gets or sets the number of months until the certificate expires.
   /// </summary>
   [Option("ExpireMonths", Required = false, HelpText = "The number of months until the certificate expires, default if not specifed is 240 months (20 Years).")]
   public int ExpireMonths { get; set; } = 240;
}
