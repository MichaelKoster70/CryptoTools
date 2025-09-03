// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace CertTools.AzureCertCore;

/// <summary>
/// Static class holding various constants for X.509 certificates OIDs.
/// </summary>
public static class Constants
{
   /// <summary>Extended Key Usage OID for Client Authentication</summary>
   public const string ServerAuthenticationEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.1";

   /// <summary>Extended Key Usage OID for Client Authentication friendly name</summary>
   public const string ServerAuthenticationEnhancedKeyUsageOidFriendlyName = "Server Authentication";

   /// <summary>Extended Key Usage OID for Client Authentication</summary>
   public const string ClientAuthenticationEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.2";

   /// <summary>Extended Key Usage OID for Client Authentication friendly name</summary>
   public const string ClientAuthenticationEnhancedKeyUsageOidFriendlyName = "Client Authentication";

   /// <summary>Extended Key Usage OID for Code Signing</summary>
   public const string CodeSigningEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.3";

   /// <summary>Extended Key Usage OID for Code Signing friendly name</summary>
   public const string CodeSigningEnhancedKeyUsageOidFriendlyName = "Code Signing";

   /// <summary>Extended Key Usage OID for ASP.NET Core HTTPS development certificate</summary>
   public const string AspNetHttpsEnhancedKeyUsageOid = "1.3.6.1.4.1.311.84.1.1";

   /// <summary>Extended Key Usage OID for ASP.NET Core HTTPS development certificate friendly name</summary>
   public const string AspNetHttpsEnhancedKeyUsageOidFriendlyName = "ASP.NET Core HTTPS development certificate";

   /// <summary>The current cert version used by ASP.NET 'dotnet dev-certs</summary>
   public const int AspNetCurrentCertificateVersion = 2;
}
