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
   private static readonly string[] _validKeyTypes = ["Ec", "EcHsm", "Rsa", "RsaHsm"];
   private static readonly string[] _validCurveNames = ["P256", "P256K", "P384", "P521"];
   private static readonly int[] _validKeySizes = [2048, 3072, 4096];

   /// <summary>
   /// Validates the options provided.
   /// - Interactive requires TenantId and ClientId
   /// - ClientSecret requires TenantId, ClientId
   /// - For OptionsCreateBase: KeyType, KeyCurveName and KeySize must be one of the supported values
   /// <typeparam name="T">Type derived from OptionBase</typeparam>
   /// <param name="options">The options instance to validate</param>
   /// <returns>null if validation fails, else the object value</returns>
   public static T? Validate<T>(this T? options) where T : OptionsBase
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

      // Validate key creation options for OptionsCreateBase-derived types
      if (result != null && options is OptionsCreateBase createOptions)
      {
         result = ValidateKeyCreationOptions(createOptions.KeyType, createOptions.KeyCurveName, createOptions.KeySize) ? result : null;
      }

      return result;
   }

   /// <summary>
   /// Validates key creation option values (KeyType, KeyCurveName, KeySize).
   /// </summary>
   /// <param name="keyType">The key type value to validate.</param>
   /// <param name="keyCurveName">The EC key curve name value to validate.</param>
   /// <param name="keySize">The RSA key size value to validate.</param>
   /// <returns><c>true</c> if all values are valid; <c>false</c> otherwise.</returns>
   public static bool ValidateKeyCreationOptions(string keyType, string keyCurveName, int keySize)
   {
      if (!_validKeyTypes.Contains(keyType, StringComparer.Ordinal))
      {
         PrintError($"Invalid KeyType '{keyType}'. Valid values are: Ec, EcHsm, Rsa, RsaHsm");
         return false;
      }

      if (!_validCurveNames.Contains(keyCurveName, StringComparer.Ordinal))
      {
         PrintError($"Invalid KeyCurveName '{keyCurveName}'. Valid values are: P256, P256K, P384, P521");
         return false;
      }

      if (!_validKeySizes.Contains(keySize))
      {
         PrintError($"Invalid KeySize '{keySize}'. Valid values are: 2048, 3072, 4096");
         return false;
      }

      return true;
   }

   private static void PrintError(string message)
   {
      ConsoleHelper.PrintToolInfo();
      Console.WriteLine($"ERROR: {message}");
      Console.WriteLine("  use --help for more information");
   }
}

