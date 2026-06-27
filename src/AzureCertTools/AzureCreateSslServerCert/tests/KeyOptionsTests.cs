// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Security.KeyVault.Certificates;
using CertTools.TestCore;

namespace CertTools.AzureCreateSslServerCert.Tests;

/// <summary>
/// Integration tests verifying that <see cref="Program.Main(string[])"/> supports cross-vault RSA signer scenarios.
/// </summary>
[Collection("KeyVault")]
public class KeyOptionsTests(KeyVaultFixture fixture) : IClassFixture<KeyVaultFixture>
{
   private const string RootSubjectName = "CN=Integration Test Root CA";
   private const int ExpireMonths = 12;

   /// <summary>
   /// Verifies cross-vault signing for RSA 4096 by creating the signer root certificate in the Premium Key Vault
   /// and creating the signed SSL server certificate in the Standard Key Vault using SignerKeyVaultUri.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD, AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID,
   /// and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Fact]
   public async Task CreateCertificate_Rsa4096_WithSignerInPremiumAndTargetInStandard_Succeeds()
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials || !TestConfiguration.HasPremiumWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for cross-vault workload identity authentication.");
      }

      // Arrange
      var standardVaultUri = fixture.CreateStandardKeyVaultUri();
      var premiumVaultUri = fixture.CreatePremiumKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();

      // 1. Create signer root CA in Premium Key Vault (RSA 4096)
      var rootCertName = KeyVaultFixture.GenerateCertificateName("xv-r4096-root");
      fixture.RegisterForCleanup(rootCertName, premiumVaultUri, credential);
      var rootArgs = CliArgumentBuilder.CreateWorkloadIdentityArgs(rootCertName, RootSubjectName, ExpireMonths, premiumVaultUri, keyType: "Rsa", exportable: false, keySize: 4096);
      var rootExitCode = await AzureCreateRootCert.Program.Main(rootArgs);
      Assert.Equal(0, rootExitCode);

      // 2. Create signed SSL server certificate in Standard Key Vault using signer from Premium Key Vault
      var certificateName = KeyVaultFixture.GenerateCertificateName("xv-r4096-ssl");
      fixture.RegisterForCleanup(certificateName, standardVaultUri, credential);
      string[] args =
      [
         "--CertificateName", certificateName,
         "--FQDN", "cross-vault.example.test",
         "--SignerCertificateName", rootCertName,
         "--SignerKeyVaultUri", premiumVaultUri.ToString(),
         "--ExpireMonth", ExpireMonths.ToString(),
         "--KeyVaultUri", standardVaultUri.ToString(),
         "--WorkloadIdentity",
         "--KeyType", "Rsa",
         "--KeySize", "4096"
      ];

      // Act
      var exitCode = await Program.Main(args);

      // Assert
      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePropertiesAsync(certificateName, standardVaultUri, credential, CertificateKeyType.Rsa, false);
   }
}
