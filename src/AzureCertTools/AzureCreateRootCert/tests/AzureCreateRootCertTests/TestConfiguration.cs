// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace CertTools.AzureCreateRootCert.Tests;

/// <summary>
/// Provides configuration for integration tests by reading values from environment variables.
/// </summary>
/// <remarks>
/// Required environment variables:
/// <list type="bullet">
///   <item><description><c>AZURE_KEYVAULT_URL_STANDARD</c> – URL of a Standard-tier Azure Key Vault.</description></item>
///   <item><description><c>AZURE_KEYVAULT_URL_PREMIUM</c> – URL of a Premium-tier Azure Key Vault (required for HSM-backed keys).</description></item>
///   <item><description><c>AZURE_CLIENT_ID</c> – Azure Entra ID Application (Client) ID.</description></item>
///   <item><description><c>AZURE_TENANT_ID</c> – Azure Entra ID Tenant ID.</description></item>
///   <item><description><c>AZURE_CLIENT_SECRET</c> – Azure Entra ID Application (Client) Secret.</description></item>
/// </list>
/// </remarks>
internal static class TestConfiguration
{
   /// <summary>Gets the Standard-tier Azure Key Vault URL.</summary>
   public static string? StandardKeyVaultUrl => Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URL_STANDARD");

   /// <summary>Gets the Premium-tier Azure Key Vault URL (required for HSM-backed keys).</summary>
   public static string? PremiumKeyVaultUrl => Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URL_PREMIUM");

   /// <summary>Gets the Azure Entra ID Application (Client) ID.</summary>
   public static string? ClientId => Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");

   /// <summary>Gets the Azure Entra ID Tenant ID.</summary>
   public static string? TenantId => Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

   /// <summary>Gets the Azure Entra ID Application (Client) Secret.</summary>
   public static string? ClientSecret => Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");

   /// <summary>
   /// Gets a value indicating whether all credentials required for client-secret authentication
   /// against the Standard Key Vault are available.
   /// </summary>
   public static bool HasClientSecretCredentials =>
      !string.IsNullOrEmpty(StandardKeyVaultUrl) &&
      !string.IsNullOrEmpty(ClientId) &&
      !string.IsNullOrEmpty(TenantId) &&
      !string.IsNullOrEmpty(ClientSecret);

   /// <summary>
   /// Gets a value indicating whether a workload identity test can be attempted.
   /// Only the Standard Key Vault URL is required; the OIDC token is injected by the GitHub Actions
   /// environment automatically.
   /// </summary>
   public static bool HasWorkloadIdentityCredentials => !string.IsNullOrEmpty(StandardKeyVaultUrl);

   /// <summary>
   /// Gets a value indicating whether all credentials required for client-secret authentication
   /// against the Premium Key Vault (for HSM-backed keys) are available.
   /// </summary>
   public static bool HasPremiumKeyVaultCredentials =>
      !string.IsNullOrEmpty(PremiumKeyVaultUrl) &&
      !string.IsNullOrEmpty(ClientId) &&
      !string.IsNullOrEmpty(TenantId) &&
      !string.IsNullOrEmpty(ClientSecret);
}
