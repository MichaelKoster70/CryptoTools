// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using CertTools.TestCore;
using Xunit;

namespace CertTools.AzureDeleteCert.Tests;

/// <summary>
/// Integration tests for <see cref="Program"/>.
/// </summary>
[CollectionDefinition(nameof(KeyVaultCollection))]
public sealed class KeyVaultCollection : ICollectionFixture<KeyVaultFixture>;

/// <summary>
/// Verifies certificate deletion behavior for the Azure delete tool.
/// </summary>
[Collection(nameof(KeyVaultCollection))]
public sealed class DeleteCertProgramTests
{
   private readonly KeyVaultFixture fixture;

   /// <summary>
   /// Initializes a new instance of the <see cref="DeleteCertProgramTests"/> class.
   /// </summary>
   /// <param name="fixture">Shared Key Vault test fixture.</param>
   public DeleteCertProgramTests(KeyVaultFixture fixture)
   {
      this.fixture = fixture;
   }

   /// <summary>
   /// Verifies that the tool deletes an existing certificate when client-secret authentication is used.
   /// </summary>
   [Fact]
   public async Task Main_ClientSecretCredentials_DeletesExistingCertificate_ReturnsZero()
   {
      if (!TestConfiguration.HasClientSecretCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_CLIENT_SECRET must be set for client-secret authentication.");
      }

      // Arrange
      var credential = fixture.CreateClientSecretCredential();
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var certificateName = await CreateTestCertificateAsync(vaultUri, credential);

      // Act
      var result = Program.Main(
      [
         "--CertificateName", certificateName,
         "--KeyVaultUri", vaultUri.ToString(),
         "--TenantId", TestConfiguration.GetTenantId(),
         "--ClientId", TestConfiguration.GetClientId(),
         "--ClientSecret", TestConfiguration.GetClientSecret()
      ]);
      
      // Assert
      Assert.Equal(0, result);

      var client = new CertificateClient(vaultUri, credential);
      var deletedCertificate = await client.GetDeletedCertificateAsync(certificateName);
      Assert.Equal(certificateName, deletedCertificate.Value.Name);
   }

   /// <summary>
   /// Verifies that the tool deletes an existing certificate when workload identity authentication is used.
   /// </summary>
   [Fact]
   public async Task WorkloadIdentityCredentials_DeletesExistingCertificate_ReturnsZero()
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      // Arrange
      var credential = fixture.CreateWorkloadIdentityCredential();
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var certificateName = await CreateTestCertificateAsync(vaultUri, credential);

      // Act
      var result = Program.Main(
      [
         "--CertificateName", certificateName,
         "--KeyVaultUri", vaultUri.ToString(),
         "--WorkloadIdentity"
      ]);

      // Assert
      Assert.Equal(0, result);

      var client = new CertificateClient(vaultUri, credential);
      var deletedCertificate = await client.GetDeletedCertificateAsync(certificateName);
      Assert.Equal(certificateName, deletedCertificate.Value.Name);
   }

   /// <summary>
   /// Verifies that invalid interactive arguments are rejected before Azure access is attempted.
   /// </summary>
   [Fact]
   public void InteractiveWithoutTenantId_ReturnsOne()
   {
      // Act
      var result = Program.Main(
      [
         "--CertificateName", "test-cert",
         "--KeyVaultUri", "https://example.vault.azure.net/",
         "--Interactive",
         "--ClientId", "client-id-only"
      ]);

      // Assert
      Assert.Equal(1, result);
   }

   private async Task<string> CreateTestCertificateAsync(Uri vaultUri, TokenCredential credential)
   {
      var client = new CertificateClient(vaultUri, credential);
      var certificateName = KeyVaultFixture.GenerateCertificateName("delete");

      var policy = new CertificatePolicy(WellKnownIssuerNames.Self, "CN=AzureDeleteCert Integration Test")
      {
         KeyType = CertificateKeyType.Rsa,
         KeySize = 2048,
         ReuseKey = false,
         Exportable = false,
         ValidityInMonths = 1
      };

      var operation = await client.StartCreateCertificateAsync(certificateName, policy);
      await operation.WaitForCompletionAsync();

      fixture.RegisterForCleanup(certificateName, vaultUri, credential);
      return certificateName;
   }
}