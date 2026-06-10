// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Core;
using CertTools.AzureCertCore;
using CommandLine;

namespace CertTools.AzureCreateIntermediateCert;

/// <summary>
/// Class holding teh application entry point.
/// </summary>
internal static class Program
{
   /// <summary>
   /// Application entry point.
   /// </summary>
   /// <param name="args">The args</param>
   static void Main(string[] args)
   {
      // Parse the command line options
      var options = Parser.Default.ParseArguments<Options>(args).Value.Validate();
      if (options == null)
      {
         return;
      }

      // Write header
      ConsoleHelper.PrintToolInfo();

      // Create the token provider
      TokenCredential credentials = options.GetTokenCredential();

      Uri keyVaultUri = new(options.KeyVaultUri);

      var cert = CertificateWorker.CreateIntermediateCertAsync(options.CertificateName, options.Subject, options.SignerCertificateName, options.ExpireMonths, options.PathLengthConstraint, keyVaultUri, credentials, options.GetKeyCreationOptions()).Result;

      Console.WriteLine($"Certificate created: name={cert}, Key Vault={keyVaultUri}");
   }
}
