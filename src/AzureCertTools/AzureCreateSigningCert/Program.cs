// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Core;
using Azure.Identity;
using CommandLine;

namespace CertTools.AzureCreateSigningCert;

internal class Program
{
   static void Main(string[] args)
   {
      Console.WriteLine("Crypto Tools - Azure Key Vault create signing certificate");

      // Parse the command line options, get at least SubjectName and Name
      var options = Parser.Default.ParseArguments<Options>(args).Value;
      if (options == null)
      {
         return;
      }

      // Create the token provider
      TokenCredential credentrials = options.Interactive ? new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions()
      {
         TenantId = options.TenantId,
         ClientId = options.ClientId
      }) : new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);

      Uri keyVaultUri = new(options.KeyVaultUri);

      CertificateWorker.CreateSigningCertificateAsync(options.CertificateName, options.Subject, options.SignerCertificateName, keyVaultUri, credentrials, options.ExpireMonth).Wait();

      Console.WriteLine($"Certificate {options.CertificateName} created in Key Vault {keyVaultUri}");
   }
}
