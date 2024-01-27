// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;
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
      string? password = options.Password ?? ReadPassword();

      if (password == null)
      {
         Console.WriteLine("No password given, aborting");
         return;
      }

      try
      {
        var thumbPrint = CertificateWorker.CreateRootCert(options.Subject, options.Name, password);
        Console.WriteLine($"Certificate created: Thumbprint=\"{thumbPrint}\", filename={options.Name}.pfx");
      }
      catch (CryptographicException ex)
      {
         Console.WriteLine($"Error creating certificate: {ex.Message}");
      }
   }

   /// <summary>
   /// Read a password from the console.
   /// </summary>
   /// <returns>The password string, null if user abort</returns>
   private static string? ReadPassword()
   {
      Console.Write("Enter password: ");
      var password = new StringBuilder();
      while (true)
      {
         var key = Console.ReadKey(true);
         switch(key.Key)
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