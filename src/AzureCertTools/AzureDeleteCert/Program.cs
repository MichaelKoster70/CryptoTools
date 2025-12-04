// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using CertTools.AzureCertCore;
using CommandLine;

namespace CertTools.AzureDeleteSigningCert;

internal static class Program
{
   static int Main(string[] args)
   {
      // Parse the command line options
      var options = Parser.Default.ParseArguments<OptionsBase>(args).Value.Validate();
      if (options == null)
      {
         return 1;
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

      var keyVaultUri = new Uri(options.KeyVaultUri);
      var client = new CertificateClient(keyVaultUri, credentials);

      try
      {
         // Start delete operation for the specified certificate name
         var operation = client.StartDeleteCertificate(options.CertificateName);
         operation.WaitForCompletion();

         Console.WriteLine($"Deleted certificate '{options.CertificateName}' from Key Vault '{options.KeyVaultUri}'.");
         return 0;
      }
      catch (Exception ex)
      {
         Console.WriteLine($"ERROR: Failed to delete certificate '{options.CertificateName}'. {ex.Message}");
         return 1;
      }
   }
}
