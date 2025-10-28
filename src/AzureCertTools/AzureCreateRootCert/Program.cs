// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Core;
using Azure.Identity;
using CertTools.AzureCertCore;
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
   static async Task Main(string[] args)
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
      TokenCredential credentials = options switch
      {
         { Interactive: true } => new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
         {
            TenantId = options.TenantId,
            ClientId = options.ClientId,
            RedirectUri = new Uri("http://localhost")
         }),
         { WorkloadIdentity: true } => new WorkloadIdentityCredential(),
         _ => new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret)
      };

      Uri keyVaultUri = new(options.KeyVaultUri);

      var cert = await CertificateWorker.CreateRootCertAsync(options.CertificateName, options.Subject, options.ExpireMonths, keyVaultUri, credentials);

      Console.WriteLine($"Certificate created: name={cert}, Key Vault={keyVaultUri}");
   }
}
