// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace CertTools.CertCore;

/// <summary>
/// Static class holding various constants used throughout the library.
/// </summary>
public static class X509Constants
{
   /// <summary>Extended Key Usage OID for Client Authentication</summary>
   public const string ServerAuthEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.1";

   /// <summary>Extended Key Usage OID for Client Authentication friendly name</summary>
   public const string ServerAuthEnhancedKeyUsageOidFriendlyName = "Server Authentication";

   /// <summary>Extended Key Usage OID for Client Authentication</summary>
   public const string ClientAuthEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.2";

   /// <summary>Extended Key Usage OID for Client Authentication friendly name</summary>
   public const string ClientAuthEnhancedKeyUsageOidFriendlyName = "Client Authentication";

   /// <summary>Extended Key Usage OID for Code Signing</summary>
   public const string CodeSigningEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.3";

   /// <summary>Extended Key Usage OID for Code Signing friendly name</summary>
   public const string CodeSigningEnhancedKeyUsageOidFriendlyName = "Code Signing";
}
