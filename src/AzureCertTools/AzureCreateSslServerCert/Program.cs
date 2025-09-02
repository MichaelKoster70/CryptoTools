// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Text;
using CommandLine;

namespace CertTools.AzureCreateSslServerCert;

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

      // Check if the signing cert PFX password is given, if not, ask for it 
      string? signingPassword = options.Password ?? ReadPassword("PFX");

      if (signingPassword == null)
      {
         Console.WriteLine("No signing cert password given, aborting");
         return;
      }

      try
      {

         Console.WriteLine($"Certificate created: filename={options.FileName}.pfx");
      }
      catch (Exception ex)
      {
         Console.WriteLine($"Error creating certificate: {ex.Message}");
      }
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
