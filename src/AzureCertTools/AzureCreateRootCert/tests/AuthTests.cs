// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Security.KeyVault.Certificates;
using CertTools.TestCore;

namespace CertTools.AzureCreateRootCert.Tests;

/// <summary>
/// Integration tests verifying that <see cref="Program.Main(string[])"/> can authenticate to Azure Key Vault
/// using different credential types and successfully create a root CA certificate.
/// </summary>
[Collection("KeyVault")]
public class AuthTests(KeyVaultFixture fixture) : IClassFixture<KeyVaultFixture>
{
   private const string SubjectName = "CN=Integration Test Root CA";
   private const int ExpireMonths = 12;

   /// <summary>
   /// Verifies end-to-end root CA certificate creation using a client-secret credential against the configured Standard Key Vault.
   /// Requires environment variables: AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_CLIENT_SECRET.
   /// </summary>
   [Fact]
   public async Task Main_WithClientSecretCredential_Succeeds()
   {
      if (!TestConfiguration.HasClientSecretCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_CLIENT_SECRET must all be set.");
      }

      // Arrange
      var certName = KeyVaultFixture.GenerateCertificateName("auth-cs");
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateClientSecretCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CreateClientSecretArgs(certName, vaultUri);

      // Act
      var exitCode = await Program.Main(args);

      // Assert
      Assert.Equal(0, exitCode);
      await AssertCertificateExistsAsync(certName, vaultUri, credential);
   }

   /// <summary>
   /// Verifies end-to-end root CA certificate creation using a workload identity credential against the configured Standard Key Vault.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Fact]
   public async Task Main_WithWorkloadIdentityCredential_Succeeds()
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      // Arrange
      var certName = KeyVaultFixture.GenerateCertificateName("auth-wi");
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CreateWorkloadIdentityArgs(certName, vaultUri);

      // Act
      var exitCode = await Program.Main(args);

      // Assert
      Assert.Equal(0, exitCode);
      await AssertCertificateExistsAsync(certName, vaultUri, credential);
   }

   private static string[] CreateClientSecretArgs(string certName, Uri vaultUri) =>
   [
      "--CertificateName", certName,
      "--Subject", SubjectName,
      "--ExpireMonths", ExpireMonths.ToString(),
      "--KeyVaultUri", vaultUri.ToString(),
      "--TenantId", TestConfiguration.GetTenantId(),
      "--ClientId", TestConfiguration.GetClientId(),
      "--ClientSecret", TestConfiguration.GetClientSecret(),
      "--KeyType", "Rsa",
      "--KeySize", "4096"
   ];

   private static string[] CreateWorkloadIdentityArgs(string certName, Uri vaultUri) =>
   [
      "--CertificateName", certName,
      "--Subject", SubjectName,
      "--ExpireMonths", ExpireMonths.ToString(),
      "--KeyVaultUri", vaultUri.ToString(),
      "--WorkloadIdentity",
      "--KeyType", "Rsa",
      "--KeySize", "4096"
   ];

   private static async Task AssertCertificateExistsAsync(string certName, Uri vaultUri, Azure.Core.TokenCredential credential)
   {
      var client = new CertificateClient(vaultUri, credential);
      var response = await client.GetCertificateAsync(certName);
      Assert.NotNull(response.Value);
      Assert.Equal(certName, response.Value.Name);
   }
}
