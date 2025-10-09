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

internal static class Program
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
   }
}
