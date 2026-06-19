// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CertTools.AzureCertCore;
using CommandLine;

namespace CertTools.AzureCreateIntermediateCert;

/// <summary>
/// Container class for the command line options.
/// </summary>
internal class Options : OptionsCreateWithSubjectBase
{
   /// <summary>
   /// Gets or sets the name of the signer certificate stored in Key Vault.
   /// </summary>
   [Option("SignerCertificateName", Required = true, HelpText = "The name of the signer certificate in Key Vault")]
   public required string SignerCertificateName { get; set; }

   /// <summary>
   /// Gets or sets the Azure Key Vault URI where the signer certificate is stored.
   /// If not specified, the signer certificate is expected to be in the same Key Vault as the certificate being created.
   /// </summary>
   [Option("SignerKeyVaultUri", Required = false, HelpText = "The URI to the Azure Key Vault holding the signer certificate. If not specified, the same Key Vault as the target certificate is used.")]
   public string? SignerKeyVaultUri { get; set; }

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
