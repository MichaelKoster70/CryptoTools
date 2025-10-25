// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using CommandLine.Text;

namespace CertTools.AzureCertCore;

/// <summary>
/// Static class holding extension methods for the command line parser.
/// </summary>
public static class OptionsExtensions
{
   /// <summary>
   /// Prints the tool heading information (same as Parser)
   /// </summary>
   public static void PrintToolInfo()
   {
      Console.WriteLine(HeadingInfo.Default);
      Console.WriteLine(CopyrightInfo.Default);
      Console.WriteLine();
   }

   /// <summary>
   /// Validates the options provided.
   /// - Interactive requires TenantId and ClientId
   /// - ClientSecret requires TenantId, ClientId
   /// <typeparam name="T">Type derived from OptionBase</typeparam>
   /// <param name="options">The options instance to validate</param>
   /// <returns>null if validation fails, else the object value</returns>
   public static T? Validate<T> (this T? options) where T : OptionsBase
   {
      T? result = options;

      // if we have no options, parsing failed => return null
      if (options == null)
      {
         return null;
      }

      if (options.Interactive && (string.IsNullOrEmpty(options.TenantId) || string.IsNullOrEmpty(options.ClientId)))
      {
         PrintError("TenantId and ClientId are required when using 'Interactive'");
         result = null;
      }
      else if (!string.IsNullOrEmpty(options.ClientSecret) && (string.IsNullOrEmpty(options.TenantId) || string.IsNullOrEmpty(options.ClientId)))
      {
         PrintError("TenantId and ClientId are required when using 'ClientSecret'");
         result = null;
      }

      return result;
   }

   private static void PrintError(string message)
   {
      PrintToolInfo();
      Console.WriteLine($"ERROR: {message}");
      Console.WriteLine("  use --help for more information");
   }
}

