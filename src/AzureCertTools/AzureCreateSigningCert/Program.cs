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

namespace CertTools.AzureCreateSigningCert;

internal static class Program
{
   static int Main(string[] args)
   {
      // Parse the command line options
      var options = Parser.Default.ParseArguments<Options>(args).Value.Validate();
      if (options == null)
      {
         return 1;
      }

      // Write header
      ConsoleHelper.PrintToolInfo();

      // Check that we have a password if a PFX file is created
      if (!string.IsNullOrEmpty(options.FileName))
      {
         // in Workload Identity access node, we cannot prompt for password. terminate with error.
         if (options.WorkloadIdentity && string.IsNullOrEmpty(options.Password))
         {
            Console.WriteLine("ERROR: Password is required when creating a PFX file");
            return 1;
         }
         else
         {
            // For other authentication methods, we prompt for password if not given on command line
            options.Password ??= ConsoleHelper.ReadPassword("signing cert");

            if (string.IsNullOrEmpty(options.Password))
            {
               Console.WriteLine("No password given, aborting");
               return 1;
            }
         }
      }

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

      if (!string.IsNullOrEmpty(options.CertificateName))
      {
         CertificateWorker.KeyVaultCreateSigningCertificateAsync(options.CertificateName, options.Subject, options.SignerCertificateName, keyVaultUri, credentials, options.ExpireMonth).Wait();
         Console.WriteLine($"Certificate created: name={options.CertificateName}, Key Vault={keyVaultUri}");
      }
      else if (!string.IsNullOrEmpty(options.FileName))
      {
         CertificateWorker.LocalCreateSigningCertificateAsync(options.FileName, options.Password, options.Subject, options.SignerCertificateName, keyVaultUri, credentials, options.ExpireMonth).Wait();
         Console.WriteLine($"PFX file created: {options.FileName}");
      }
      else
      {
         Console.WriteLine("Either CertificateName or FileName must be specified");
      }

      return 0;
   }
}
