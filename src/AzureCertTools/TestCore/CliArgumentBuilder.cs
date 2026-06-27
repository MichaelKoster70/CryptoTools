// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace CertTools.TestCore;

/// <summary>
/// Static helper class for building command line arguments for test scenarios.
/// </summary>
public static class CliArgumentBuilder
{
   public static string[] CreateWorkloadIdentityArgs(string certName, string subjectName, int expireMonths, Uri vaultUri, string keyType, bool exportable, int? keySize = null, string? keyCurveName = null, int? pathLengthConstraint = null, string? signerCertificateName = null, Uri? signerVaultUri = null)
   {
      ArgumentNullException.ThrowIfNull(certName);
      ArgumentNullException.ThrowIfNull(vaultUri);
      ArgumentException.ThrowIfNullOrWhiteSpace(keyType);

      var args = new List<string>
      {
         "--CertificateName", certName,
         "--Subject", subjectName,
         "--ExpireMonths", expireMonths.ToString(),
         "--KeyVaultUri", vaultUri.ToString(),
         "--WorkloadIdentity",
         "--KeyType", keyType
      };

      if (exportable)
      {
         args.Add("--Exportable");
      }

      if (pathLengthConstraint.HasValue)
      {
         args.Add("--PathLengthConstraint");
         args.Add(pathLengthConstraint.Value.ToString());
      }

      if (!string.IsNullOrWhiteSpace(signerCertificateName))
      {
         args.Add("--SignerCertificateName");
         args.Add(signerCertificateName);
      }

      if (signerVaultUri != null)
      {
         args.Add("--SignerKeyVaultUri");
         args.Add(signerVaultUri.ToString());
      }

      switch (keyType)
      {
         case "Rsa":
         case "RsaHsm":
            args.Add("--KeySize");
            args.Add((keySize ?? throw new ArgumentNullException(nameof(keySize))).ToString());
            break;
         case "Ec":
         case "EcHsm":
            args.Add("--KeyCurveName");
            args.Add(keyCurveName ?? throw new ArgumentNullException(nameof(keyCurveName)));
            break;
         default:
            throw new NotSupportedException($"Unsupported key type '{keyType}'.");
      }

      return [.. args];
   }
}
