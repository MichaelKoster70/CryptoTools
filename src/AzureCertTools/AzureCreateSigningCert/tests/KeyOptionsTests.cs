// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Security.KeyVault.Certificates;
using CertTools.TestCore;

namespace CertTools.AzureCreateSigningCert.Tests;

/// <summary>
/// Integration tests verifying that <see cref="Program.Main(string[])"/> correctly creates signing certificates
/// for every supported key type, key size, and EC curve combination.
/// </summary>
[Collection("KeyVault")]
public class KeyOptionsTests(KeyVaultFixture fixture) : IClassFixture<KeyVaultFixture>
{
   private const string SubjectName = "CN=Signing Test";
   private const string RootSubjectName = "CN=Integration Test Root CA";
   private const int ExpireMonths = 12;

   // ------------------------------------------------------------------
   // HSM-backed RSA  (requires Premium Key Vault)
   // ------------------------------------------------------------------

   /// <summary>Provides RSA HSM key sizes supported by Azure Key Vault.</summary>
   public static TheoryData<int, string> RsaHsmKeyOptions()
   {
      var data = new TheoryData<int, string>
      {
         { 2048, "irh2048ne" },
         { 3072, "irh3072ne" },
         { 4096, "irh4096ne" }
      };
      return data;
   }

   /// <summary>
   /// Verifies that an HSM-backed RSA signing CA certificate is created in the Premium Key Vault
   /// for each supported RSA key size (2048, 3072, 4096 bits) and exportability setting.
   /// Requires: AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID, and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Theory]
   [MemberData(nameof(RsaHsmKeyOptions))]
   public async Task CreateCertificate_RsaHsmKey_Succeeds(int keySize, string prefix)
   {
      const bool Exportable = false; // RSA HSM keys only support non-exportable keys

      if (!TestConfiguration.HasPremiumWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      // Arrange
      var vaultUri = fixture.CreatePremiumKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();

      // Act
      // 1. create a root CA to sign the intermediate
      var rootCertName = KeyVaultFixture.GenerateCertificateName($"{prefix}-root");
      fixture.RegisterForCleanup(rootCertName, vaultUri, credential);
      var rootArgs = CliArgumentBuilder.CreateWorkloadIdentityArgs(rootCertName, RootSubjectName, ExpireMonths, vaultUri, keyType: "RsaHsm", exportable: Exportable, keySize: 4096);
      var rootExitCode = await AzureCreateRootCert.Program.Main(rootArgs);
      Assert.Equal(0, rootExitCode);

      // 2. create the intermediate cert signed by the root
      var intermediateCertName = KeyVaultFixture.GenerateCertificateName(prefix);
      fixture.RegisterForCleanup(intermediateCertName, vaultUri, credential);
      var intermediateArgs = CliArgumentBuilder.CreateWorkloadIdentityArgs(intermediateCertName, SubjectName, ExpireMonths, vaultUri, keyType: "RsaHsm", exportable: Exportable, keySize: keySize, signerCertificateName: rootCertName);

      var exitCode = await Program.Main(intermediateArgs);

      // Assert
      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePropertiesAsync(intermediateCertName, vaultUri, credential, CertificateKeyType.RsaHsm, Exportable);
   }

   // ------------------------------------------------------------------
   // Software-backed RSA  (Standard Key Vault)
   // ------------------------------------------------------------------

   /// <summary>
   /// Verifies that a software-backed RSA 4096-bit intermediate CA certificate is created in the Standard Key Vault
   /// for both exportable and non-exportable key settings.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Theory]
   [InlineData(false, "ir4096ne")]
   [InlineData(true, "ir4096e")]
   public async Task CreateCertificate_RsaKey_Succeeds(bool exportable, string prefix)
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      // Arrange
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();

      // Act
      // 1. create a root CA to sign the intermediate
      var rootCertName = KeyVaultFixture.GenerateCertificateName($"{prefix}-root");
      fixture.RegisterForCleanup(rootCertName, vaultUri, credential);
      var rootArgs = CliArgumentBuilder.CreateWorkloadIdentityArgs(rootCertName, RootSubjectName, ExpireMonths, vaultUri, keyType: "Rsa", exportable: false, keySize: 4096);
      var rootExitCode = await AzureCreateRootCert.Program.Main(rootArgs);
      Assert.Equal(0, rootExitCode);

      // 2. create the intermediate cert signed by the root
      var intermediateCertName = KeyVaultFixture.GenerateCertificateName(prefix);
      fixture.RegisterForCleanup(intermediateCertName, vaultUri, credential);
      var intermediateArgs = CliArgumentBuilder.CreateWorkloadIdentityArgs(intermediateCertName, SubjectName, ExpireMonths, vaultUri, keyType: "Rsa", exportable: exportable, keySize: 4096, signerCertificateName: rootCertName);
      var exitCode = await Program.Main(intermediateArgs);

      // Assert
      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePropertiesAsync(intermediateCertName, vaultUri, credential, CertificateKeyType.Rsa, exportable);
   }

   /// <summary>
   /// Verifies cross-vault signing for RSA 4096 by creating the signer root certificate in the Premium Key Vault
   /// and creating the signed certificate in the Standard Key Vault using SignerKeyVaultUri.
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

      // 2. Create signed certificate in Standard Key Vault using signer from Premium Key Vault
      var certificateName = KeyVaultFixture.GenerateCertificateName("xv-r4096-sign");
      fixture.RegisterForCleanup(certificateName, standardVaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(
         certName: certificateName,
         subjectName: SubjectName,
         expireMonths: ExpireMonths,
         vaultUri: standardVaultUri,
         keyType: "Rsa",
         exportable: false,
         keySize: 4096,
         signerCertificateName: rootCertName,
         signerVaultUri: premiumVaultUri);

      // Act
      var exitCode = await Program.Main(args);

      // Assert
      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePropertiesAsync(certificateName, standardVaultUri, credential, CertificateKeyType.Rsa, false);
   }

   /// <summary>
   /// Verifies that command-line validation fails when exportable is requested for HSM-backed key types.
   /// </summary>
   [Theory]
   [InlineData("RsaHsm", 2048, null)]
   [InlineData("EcHsm", null, "P256")]
   public async Task CreateCertificate_ExportableHsmKey_FailsValidation(string keyType, int? keySize, string? keyCurveName)
   {
      // Arrange
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(
         certName: "test-cert",
         subjectName: SubjectName,
         expireMonths: ExpireMonths,
         vaultUri: new Uri("https://unit-test.vault.azure.net/"),
         keyType: keyType,
         exportable: true,
         keySize: keySize,
         keyCurveName: keyCurveName,
         signerCertificateName: "root-ca");

      // Act
      var exitCode = await Program.Main(args);

      // Assert
      Assert.Equal(1, exitCode);
   }

   [Fact]
   public async Task CreateCertificate_FilenameWithoutPasswordForPfx_ReturnsError()
   {
      string[] args =
      [
         "--FileName", "test-signing-cert.pfx",
         "--Subject", "CN=Signing Test",
         "--SignerCertificateName", "root-ca",
         "--KeyVaultUri", "https://unit-test.vault.azure.net/",
         "--WorkloadIdentity",
         "--KeyType", "Rsa",
         "--KeySize", "4096"
      ];

      var exitCode = await Program.Main(args);

      Assert.Equal(1, exitCode);
   }
}
