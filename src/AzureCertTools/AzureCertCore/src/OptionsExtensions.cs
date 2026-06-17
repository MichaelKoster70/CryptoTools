// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Core;
using Azure.Identity;

namespace CertTools.AzureCertCore;

/// <summary>
/// Static class holding extension methods for the command line parser.
/// </summary>
public static class OptionsExtensions
{
   private static readonly string[] validKeyTypes = ["Ec", "EcHsm", "Rsa", "RsaHsm"];
   private static readonly string[] validCurveNames = ["P256", "P256K", "P384", "P521"];
   private static readonly int[] validKeySizes = [2048, 3072, 4096];

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
         result = ValidateKeyCreationOptions(createOptions.KeyType, createOptions.KeyCurveName, createOptions.KeySize, createOptions.Exportable) ? result : null;
      }

      return result;
   }

   /// <summary>
   /// Validates key creation option values (KeyType, KeyCurveName, KeySize).
   /// </summary>
   /// <param name="keyType">The key type value to validate.</param>
   /// <param name="keyCurveName">The EC key curve name value to validate.</param>
   /// <param name="keySize">The RSA key size value to validate.</param>
   /// <param name="exportable">A value indicating whether the key is requested to be exportable.</param>
   /// <returns><c>true</c> if all values are valid; <c>false</c> otherwise.</returns>
   public static bool ValidateKeyCreationOptions(string keyType, string keyCurveName, int keySize, bool exportable = false)
   {
      ArgumentNullException.ThrowIfNull(keyType);
      ArgumentNullException.ThrowIfNull(keyCurveName);

      keyType = keyType.Trim();
      keyCurveName = keyCurveName.Trim();

      if (!validKeyTypes.Contains(keyType, StringComparer.OrdinalIgnoreCase))
      {
         PrintError($"Invalid KeyType '{keyType}'. Valid values are: Ec, EcHsm, Rsa, RsaHsm");
         return false;
      }

      bool isHsmKey = keyType.Equals("EcHsm", StringComparison.OrdinalIgnoreCase) || keyType.Equals("RsaHsm", StringComparison.OrdinalIgnoreCase);
      if (exportable && isHsmKey)
      {
         PrintError($"Exportable keys are not supported with HSM-backed key type '{keyType}'.");
         return false;
      }

      bool isEcKey = keyType.Equals("Ec", StringComparison.OrdinalIgnoreCase) || keyType.Equals("EcHsm", StringComparison.OrdinalIgnoreCase);
      if (isEcKey)
      {
         if (!validCurveNames.Contains(keyCurveName, StringComparer.OrdinalIgnoreCase))
         {
            PrintError($"Invalid KeyCurveName '{keyCurveName}'. Valid values are: P256, P256K, P384, P521");
            return false;
         }
      }
      else
      {
         if (!validKeySizes.Contains(keySize))
         {
            PrintError($"Invalid KeySize '{keySize}'. Valid values are: 2048, 3072, 4096");
            return false;
         }
      }

      return true;
   }

   /// <summary>
   /// Converts the key creation related options from the provided OptionsCreateBase instance into a <see cref="KeyCreationOptions"/> object.
   /// </summary>
   /// <param name="options">The OptionsCreateBase instance containing the key creation options.</param>
   /// <returns>A <see cref="KeyCreationOptions"/> object representing the key creation options.</returns>
   public static KeyCreationOptions GetKeyCreationOptions(this OptionsCreateBase options)
   {
      ArgumentNullException.ThrowIfNull(options);

      string keyType = options.KeyType.Trim().ToUpperInvariant();
      string keyCurveName = options.KeyCurveName.Trim().ToUpperInvariant();

      return keyType switch
      {
         "EC" or "ECHSM" => new EcKeyCreationOptions
         {
            Exportable = options.Exportable,
            KeyCurve = keyCurveName switch
            {
               "P256" => EcKeyCurve.P256,
               "P256K" => EcKeyCurve.P256K,
               "P384" => EcKeyCurve.P384,
               _ => EcKeyCurve.P521,
            },
            HsmBacked = keyType.Equals("ECHSM", StringComparison.OrdinalIgnoreCase)
         },
         "RSA" or "RSAHSM" => new RsaKeyCreationOptions
         {
            Exportable = options.Exportable,
            KeySize = options.KeySize,
            HsmBacked = keyType.Equals("RSAHSM", StringComparison.OrdinalIgnoreCase)
         },
         _ => throw new NotSupportedException($"Unsupported key type '{options.KeyType}'. Supported values are: Ec, EcHsm, Rsa, RsaHsm.")
      };
   }

   /// <summary>
   /// Returns an appropriate <see cref="TokenCredential"/> instance based on the authentication options provided in the <see cref="OptionsBase"/> instance.
   /// </summary>
   /// <param name="options">The <see cref="OptionsBase"/> instance containing the authentication options.</param>
   /// <returns>A <see cref="TokenCredential"/> instance.</returns>
   public static TokenCredential GetTokenCredential(this OptionsBase options) => options switch
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

   private static void PrintError(string message)
   {
      ConsoleHelper.PrintToolInfo();
      Console.WriteLine($"ERROR: {message}");
      Console.WriteLine("  use --help for more information");
   }
}

