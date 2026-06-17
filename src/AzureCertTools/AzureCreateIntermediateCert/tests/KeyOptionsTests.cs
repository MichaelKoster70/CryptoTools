// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Certificates;
using CertTools.TestCore;

namespace CertTools.AzureCreateIntermediateCert.Tests;

/// <summary>
/// Integration tests verifying that <see cref="Program.Main(string[])"/> correctly creates intermediate CA certificates
/// for every supported key type, key size, and EC curve combination.
/// </summary>
[Collection("KeyVault")]
public class KeyOptionsTests(KeyVaultFixture fixture) : IClassFixture<KeyVaultFixture>
{
   private const string IntermediateSubjectName = "CN=Integration Test Intermediate CA";
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
   /// Verifies that an HSM-backed RSA intermediate CA certificate is created in the Premium Key Vault
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
      var intermediateArgs = CliArgumentBuilder.CreateWorkloadIdentityArgs(intermediateCertName, IntermediateSubjectName, ExpireMonths, vaultUri, keyType: "RsaHsm", exportable: Exportable, keySize: keySize, signerCertificateName: rootCertName);

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
      var intermediateArgs = CliArgumentBuilder.CreateWorkloadIdentityArgs(intermediateCertName, IntermediateSubjectName, ExpireMonths, vaultUri, keyType: "Rsa", exportable: exportable, keySize: 4096, signerCertificateName: rootCertName);
      var exitCode = await Program.Main(intermediateArgs);

      // Assert
      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePropertiesAsync(intermediateCertName, vaultUri, credential, CertificateKeyType.Rsa, exportable);
   }

   /// <summary>
   /// Verifies that the intermediate CA certificate contains or omits a path length constraint
   /// based on whether the command line argument is provided.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Theory]
   [InlineData(null, "iplnone")]
   [InlineData(0, "iplzero")]
   [InlineData(1, "iplone")]
   public async Task CreateCertificate_PathLengthConstraint_PresentAndAbsent_Succeeds(int? pathLengthConstraint, string prefix)
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      // Arrange
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();

      // Act
      // 1. create a root CA to sign the intermediate (with path length constraint of 1 to allow intermediate)
      var rootCertName = KeyVaultFixture.GenerateCertificateName($"{prefix}-root");
      fixture.RegisterForCleanup(rootCertName, vaultUri, credential);
      var rootArgs = CliArgumentBuilder.CreateWorkloadIdentityArgs(rootCertName, RootSubjectName, ExpireMonths, vaultUri, keyType: "Rsa", exportable: false, keySize: 4096, pathLengthConstraint: 1);
      var rootExitCode = await AzureCreateRootCert.Program.Main(rootArgs);
      Assert.Equal(0, rootExitCode);

      // 2. create the intermediate cert signed by the root
      var intermediateCertName = KeyVaultFixture.GenerateCertificateName(prefix);
      fixture.RegisterForCleanup(intermediateCertName, vaultUri, credential);
      var intermediateArgs = CliArgumentBuilder.CreateWorkloadIdentityArgs(intermediateCertName, IntermediateSubjectName, ExpireMonths, vaultUri, keyType: "Rsa", exportable: false, keySize: 4096, pathLengthConstraint: pathLengthConstraint, signerCertificateName: rootCertName);

      var exitCode = await Program.Main(intermediateArgs);

      // Assert
      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePathLengthConstraintAsync(intermediateCertName, vaultUri, credential, pathLengthConstraint);
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
         subjectName: IntermediateSubjectName,
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
}

