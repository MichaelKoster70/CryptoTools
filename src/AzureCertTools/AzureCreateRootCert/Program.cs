// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Core;
using Azure.Identity;
using CommandLine;

namespace CertTools.AzureCreateRootCert;

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
      Console.WriteLine("Crypto Tools - Azure Key Vault create root certificate");

      // Parse the command line options, get at least SubjectName and Name
      var options = Parser.Default.ParseArguments<Options>(args).Value;
      if (options == null)
      {
         return;
      }

      TokenCredential credentrials = options.Interactive ? new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions()
      {
         TenantId = options.TenantId,
         ClientId = options.ClientId
      }) : new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);

      Uri keyVaultUri = new(options.KeyVaultUri);

      var cert = CertificateWorker.CreateRootCertAsync(options.Name, options.Subject, options.ExpireMonth, keyVaultUri, credentrials).Result;

      Console.WriteLine($"Certificate {cert} created in Key Vault {keyVaultUri}");
   }
}
