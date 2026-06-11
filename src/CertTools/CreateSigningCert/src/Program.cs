// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CertTools.CertCore;
using CommandLine;

namespace CertTools.CreateSigningCert;

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
      var options = Parser.Default.ParseArguments<Options>(args).Value;
      if (options == null)
      {
         return;
      }

      // Write header
      OptionsExtensions.PrintToolInfo();

      // Check if the signing cert PFX password is given, if not, ask for it 
      string? signingPassword = options.Password ?? ConsoleHelper.ReadPassword("signing cert");

      if (signingPassword == null)
      {
         Console.WriteLine("No signing cert password given, aborting");
         return;
      }
      try
      {
         if (options.SignerThumbprint != null)
         {
            CertificateWorker.CreateSigningCertificate(options.Subject, options.Name, signingPassword, options.SignerThumbprint, options.ExpireMonths);
         }
         else
         {
            // Check if the root cert PFX password is given, if not, ask for it 
            string? rootPassword = options.Password ?? ConsoleHelper.ReadPassword("root cert");

            if (rootPassword == null)
            {
               Console.WriteLine("No root cert password given, aborting");
               return;
            }

            CertificateWorker.CreateSigningCertificate(options.Subject, options.Name, signingPassword, options.SignerPfx, rootPassword, options.ExpireMonths);
         }

         Console.WriteLine($"Certificate created: filename={options.Name}.pfx");
      }
      catch (Exception ex)
      {
         Console.WriteLine($"Error creating certificate: {ex.Message}");
      }
   }
}
