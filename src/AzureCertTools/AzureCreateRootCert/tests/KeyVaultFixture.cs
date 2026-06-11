// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;

namespace CertTools.AzureCreateRootCert.Tests;

/// <summary>
/// xUnit class fixture that provides shared Azure Key Vault test infrastructure and
/// handles certificate cleanup after all tests in the collection have run.
/// </summary>
public sealed class KeyVaultFixture : IAsyncLifetime
{
   private readonly List<(string Name, Uri VaultUri, TokenCredential Credential)> _registeredCertificates = [];

   /// <summary>Returns a <see cref="ClientSecretCredential"/> built from the configured environment variables.</summary>
   public TokenCredential CreateClientSecretCredential() =>
      new ClientSecretCredential(
         TestConfiguration.GetTenantId(),
         TestConfiguration.GetClientId(),
         TestConfiguration.GetClientSecret());

   /// <summary>Returns a <see cref="WorkloadIdentityCredential"/> for GitHub Actions OIDC authentication.</summary>
   public TokenCredential CreateWorkloadIdentityCredential() => new WorkloadIdentityCredential();

   /// <summary>Returns the Standard-tier Key Vault URI from the configured environment variable.</summary>
   public Uri CreateStandardKeyVaultUri() => new Uri(TestConfiguration.GetStandardKeyVaultUrl());

   /// <summary>Returns the Premium-tier Key Vault URI from the configured environment variable.</summary>
   public Uri CreatePremiumKeyVaultUri() => new Uri(TestConfiguration.GetPremiumKeyVaultUrl());

   /// <summary>
   /// Registers a certificate for deletion when the test collection finishes.
   /// </summary>
   /// <param name="certificateName">Azure Key Vault certificate name.</param>
   /// <param name="vaultUri">URI of the Key Vault that contains the certificate.</param>
   /// <param name="credential">Credential with permission to delete from that vault.</param>
   public void RegisterForCleanup(string certificateName, Uri vaultUri, TokenCredential credential) =>
      _registeredCertificates.Add((certificateName, vaultUri, credential));

   /// <summary>
   /// Generates a unique, Azure Key Vault-compatible certificate name for a test.
   /// Format: <c>it-{prefix}-{32-char hex GUID}</c>.
   /// </summary>
   /// <param name="prefix">Short identifier describing the test scenario (e.g. <c>rh2048</c> for RSA HSM 2048).</param>
   public static string GenerateCertificateName(string prefix) =>
      $"it-{prefix}-{Guid.NewGuid():N}";

   ValueTask IAsyncLifetime.InitializeAsync() => ValueTask.CompletedTask;

   async ValueTask IAsyncDisposable.DisposeAsync()
   {
      foreach (var (name, vaultUri, credential) in _registeredCertificates)
      {
         try
         {
            var client = new CertificateClient(vaultUri, credential);
            var deleteOperation = await client.StartDeleteCertificateAsync(name);
            await deleteOperation.WaitForCompletionAsync();

            // Attempt to purge so the vault does not accumulate soft-deleted certificates.
            // Purge requires the 'Certificate Purge' permission and may not be available in all environments.
            try
            {
               await client.PurgeDeletedCertificateAsync(name);
            }
            catch (Exception)
            {
               // Ignore: purge permission may not be granted, or soft-delete may not be enabled
            }
         }
         catch (Exception)
         {
            // Ignore: certificate may not exist (e.g. the test that created it failed before completing)
         }
      }
   }
}

/// <summary>
/// xUnit collection definition that shares a single <see cref="KeyVaultFixture"/> instance
/// across all Key Vault integration test classes.
/// </summary>
[CollectionDefinition("KeyVault")]
public class KeyVaultCollection : ICollectionFixture<KeyVaultFixture>
{
}
