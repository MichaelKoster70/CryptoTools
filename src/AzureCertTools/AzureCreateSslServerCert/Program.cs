// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Text;
using Azure.Core;
using Azure.Identity;
using CommandLine;

namespace CertTools.AzureCreateSslServerCert;

/// <summary>
/// Class holding the application entry point.
/// </summary>
internal static class Program
{
   /// <summary>
   /// Application entry point.
   /// </summary>
   /// <param name="args">The args</param>
   static async Task<int> Main(string[] args)
   {
      Console.WriteLine("Crypto Tools - Azure Key Vault create SSL Server certificate");

      int result = 1;

      // Parse the command line options, get at least SubjectName and Name
      var options = Parser.Default.ParseArguments<Options>(args).Value;
      if (options == null)
      {
         return result;
      }

      if (options.Local)
      {
         Console.WriteLine("Creating certificate locally");

         // Check if the signing cert PFX password is given, if not, ask for it 
         string? signingPassword = options.Password ?? ReadPassword("PFX");

         if (signingPassword == null)
         {
            Console.WriteLine("No signing cert password given, aborting");
            return result;
         }
      }
      else
      {
         Console.WriteLine("Creating certificate in Azure Key Vault");
      }

      try
      {
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

         var resultName = await CertificateWorker.CreateSslServerCertificateAsync(options.CertificateName, options.FQDN, options.SignerCertificateName, keyVaultUri, credentials, options.ExpireMonth, options.Local, options.Password);
         Console.WriteLine($"Certificate created: {resultName}");

         result = 0;
      }
      catch (Exception ex)
      {
         Console.WriteLine($"Error creating certificate: {ex.Message}");
         result = 1;
      }

      return result;
   }

   /// <summary>
   /// Read a password from the console.
   /// </summary>
   /// <param name="kind">The kind of password</param>
   /// <returns>The password string, null if user abort</returns>
   private static string? ReadPassword(string kind)
   {
      Console.Write($"Enter {kind} password: ");
      var password = new StringBuilder();
      while (true)
      {
         var key = Console.ReadKey(true);
         switch (key.Key)
         {
            case ConsoleKey.Escape:
               Console.WriteLine();
               return null;
            case ConsoleKey.Enter:
               Console.WriteLine();
               return password.ToString();
            case ConsoleKey.Backspace:
               if (password.Length > 0)
               {
                  password = password.Remove(password.Length - 1, 1);
                  Console.Write("\b \b");
               }
               break;
            default:
               _ = password.Append(key.KeyChar);
               Console.Write("*");
               break;
         }
      }
   }
}
