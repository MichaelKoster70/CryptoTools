// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CertTools.CertCore;
using CommandLine;

namespace CertTools.CreateRootCert;

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
      Console.WriteLine($"Crypto Tools - create root certificate");

      // Parse the command line options, get at least SubjectName and Name
      var options = Parser.Default.ParseArguments<Options>(args).Value;
      if (options == null)
      {
         return;
      }

      // Check if the password is given, if not, ask for it 
      string? password = options.Password ?? ConsoleHelper.ReadPassword("root cert");

      if (password == null)
      {
         Console.WriteLine("No password given, aborting");
         return;
      }

      try
      {
        var thumbPrint = CertificateWorker.CreateRootCert(options.Subject, options.Name, password, options.ExpireMonth);
        Console.WriteLine($"Certificate created: Thumbprint=\"{thumbPrint}\", filename={options.Name}.pfx");
      }
      catch (Exception ex)
      {
         Console.WriteLine($"Error creating certificate: {ex.Message}");
      }
   }
}