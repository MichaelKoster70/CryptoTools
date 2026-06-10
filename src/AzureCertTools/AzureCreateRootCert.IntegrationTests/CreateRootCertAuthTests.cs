// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Security.KeyVault.Certificates;
using CertTools.AzureCertCore;

namespace CertTools.AzureCreateRootCert.IntegrationTests;

/// <summary>
/// Integration tests verifying that <see cref="CertificateWorker"/> can authenticate to Azure Key Vault
/// using different credential types and successfully create a root CA certificate.
/// </summary>
[Collection("KeyVault")]
public class CreateRootCertAuthTests(KeyVaultFixture fixture)
{
   private const string SubjectName = "CN=Integration Test Root CA";
   private const int ExpireMonths = 12;

   /// <summary>
   /// Verifies end-to-end root CA certificate creation using a client-secret credential
   /// against the configured Standard Key Vault.
   /// Requires environment variables: AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID,
   /// AZURE_TENANT_ID, AZURE_CLIENT_SECRET.
   /// </summary>
   [SkippableFact]
   public async Task CreateRootCertAsync_WithClientSecretCredential_Succeeds()
   {
      Skip.If(!TestConfiguration.HasClientSecretCredentials,
         "Skipped: AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_CLIENT_SECRET must all be set.");

      // Arrange
      var certName = KeyVaultFixture.GenerateCertificateName("auth-cs");
      var vaultUri = fixture.GetStandardKeyVaultUri();
      var credential = fixture.GetClientSecretCredential();
      var keyOptions = new RsaKeyCreationOptions { KeySize = 4096 };
      fixture.RegisterForCleanup(certName, vaultUri, credential);

      // Act
      var result = await CertificateWorker.CreateRootCertAsync(
         certName, SubjectName, ExpireMonths, pathLengthConstraint: null, vaultUri, credential, keyOptions);

      // Assert
      Assert.Equal(certName, result);
      await AssertCertificateExistsAsync(certName, vaultUri, credential);
   }

   /// <summary>
   /// Verifies end-to-end root CA certificate creation using a workload identity credential
   /// against the configured Standard Key Vault.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD and a GitHub Actions OIDC token (id-token: write permission).
   /// </summary>
   [SkippableFact]
   public async Task CreateRootCertAsync_WithWorkloadIdentityCredential_Succeeds()
   {
      Skip.If(!TestConfiguration.HasWorkloadIdentityCredentials,
         "Skipped: AZURE_KEYVAULT_URL_STANDARD must be set and the runner must have a workload identity configured.");

      // Arrange
      var certName = KeyVaultFixture.GenerateCertificateName("auth-wi");
      var vaultUri = fixture.GetStandardKeyVaultUri();
      var credential = fixture.GetWorkloadIdentityCredential();
      var keyOptions = new RsaKeyCreationOptions { KeySize = 4096 };
      fixture.RegisterForCleanup(certName, vaultUri, credential);

      // Act
      var result = await CertificateWorker.CreateRootCertAsync(
         certName, SubjectName, ExpireMonths, pathLengthConstraint: null, vaultUri, credential, keyOptions);

      // Assert
      Assert.Equal(certName, result);
      await AssertCertificateExistsAsync(certName, vaultUri, credential);
   }

   private static async Task AssertCertificateExistsAsync(string certName, Uri vaultUri, Azure.Core.TokenCredential credential)
   {
      var client = new CertificateClient(vaultUri, credential);
      var response = await client.GetCertificateAsync(certName);
      Assert.NotNull(response.Value);
      Assert.Equal(certName, response.Value.Name);
   }
}
