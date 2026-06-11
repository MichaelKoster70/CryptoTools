// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Text;

namespace CertTools.CertCore;

/// <summary>
/// Static helper class for console applications.
/// </summary>
public static class ConsoleHelper
{
   /// <summary>
   /// Read a password from the console.
   /// </summary>
   /// <param name="kind">The kind of password</param>
   /// <returns>The password string, null if user abort</returns>
   public static string? ReadPassword(string kind)
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
