// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Certificates;
using CertTools.TestCore;
namespace CertTools.AzureCreateRootCert.Tests;

/// <summary>
/// Integration tests verifying that <see cref="Program.Main(string[])"/> correctly creates root CA certificates
/// for every supported key type, key size, and EC curve combination.
/// </summary>
[Collection("KeyVault")]
public class KeyOptionsTests(KeyVaultFixture fixture) : IClassFixture<KeyVaultFixture>
{
   private const string SubjectName = "CN=Integration Test Root CA";
   private const int ExpireMonths = 12;

   // ------------------------------------------------------------------
   // HSM-backed RSA  (requires Premium Key Vault)
   // ------------------------------------------------------------------

   /// <summary>Provides RSA HSM key sizes supported by Azure Key Vault.</summary>
   public static TheoryData<int, string> RsaHsmKeyOptions()
   {
      var data = new TheoryData<int, string>
      {
         { 2048, "rh2048ne" },
         { 3072, "rh3072ne" },
         { 4096, "rh4096ne" }
      };
      return data;
   }

   /// <summary>
   /// Verifies that an HSM-backed RSA root CA certificate is created in the Premium Key Vault
   /// for each supported RSA key size (2048, 3072, 4096 bits) and exportability setting.
   /// Requires: AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID, and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Theory]
   [MemberData(nameof(RsaHsmKeyOptions))]
   public async Task CreateCertificate_RsaHsmKey_Succeeds(int keySize, string prefix)
   {
      const bool Exportable = false; // RSA HSM keys only support non-exportable keys per issue
 
      if (!TestConfiguration.HasPremiumWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreatePremiumKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "RsaHsm", exportable: Exportable, keySize: keySize);

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePropertiesAsync(certName, vaultUri, credential, CertificateKeyType.RsaHsm, Exportable);
   }

   // ------------------------------------------------------------------
   // HSM-backed EC  (requires Premium Key Vault)
   // ------------------------------------------------------------------

   /// <summary>Provides EC HSM key curves supported by Azure Key Vault.</summary>
   public static TheoryData<string, string> EcHsmKeyOptions()
   {
      var data = new TheoryData<string, string>
      {
         { "P256", "ehp256ne" },
         { "P256K", "ehp256kne" },
         { "P384", "ehp384ne" },
         { "P521", "ehp521ne" }
      };
      return data;
   }

   /// <summary>
   /// Verifies that an HSM-backed EC root CA certificate is created in the Premium Key Vault
   /// for each supported EC curve (P-256, P-256K, P-384, P-521) with the supported non-exportable HSM setting.
   /// Requires: AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID, and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Theory]
   [MemberData(nameof(EcHsmKeyOptions))]
   public async Task CreateCertificate_EcHsmKey_Succeeds(string keyCurveName,string prefix)
   {
      const bool Exportable = false; // EC HSM keys only support non-exportable keys

      if (!TestConfiguration.HasPremiumWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreatePremiumKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "EcHsm", exportable: Exportable, keyCurveName: keyCurveName);

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePropertiesAsync(certName, vaultUri, credential, CertificateKeyType.EcHsm, Exportable);
   }

   // ------------------------------------------------------------------
   // Software-backed RSA  (Standard Key Vault, 4096-bit only)
   // ------------------------------------------------------------------

   /// <summary>
   /// Verifies that a software-backed RSA 4096-bit root CA certificate is created in the Standard Key Vault
   /// for both exportable and non-exportable key settings.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Theory]
   [InlineData(false, "r4096ne")]
   [InlineData(true, "r4096ex")]
   public async Task CreateCertificate_RsaSoftwareKey_4096_Succeeds(bool exportable, string prefix)
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "Rsa", exportable, keySize: 4096);

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePropertiesAsync(certName, vaultUri, credential, CertificateKeyType.Rsa, exportable);
   }

   // ------------------------------------------------------------------
   // Software-backed EC  (Standard Key Vault, P-384)
   // ------------------------------------------------------------------

   /// <summary>
   /// Verifies that a software-backed EC P-384 root CA certificate is created in the Standard Key Vault
   /// for both exportable and non-exportable key settings.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Theory]
   [InlineData(false, "ep384ne")]
   [InlineData(true, "ep384ex")]
   public async Task CreateCertificate_EcSoftwareKey_P384_Succeeds(bool exportable, string prefix)
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "Ec", exportable, keyCurveName: "P384");

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePropertiesAsync(certName, vaultUri, credential, CertificateKeyType.Ec, exportable);
   }

   /// <summary>
   /// Verifies that the root CA certificate contains or omits a path length constraint
   /// based on whether the command line argument is provided.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and a valid AZURE_FEDERATED_TOKEN_FILE.
   /// </summary>
   [Theory]
   [InlineData(null, "plnone")]
   [InlineData(1, "plone")]
   public async Task CreateCertificate_PathLengthConstraint_PresentAndAbsent_Succeeds(int? pathLengthConstraint, string prefix)
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_FEDERATED_TOKEN_FILE must be set for workload identity authentication.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "Rsa", exportable: false, keySize: 4096, pathLengthConstraint: pathLengthConstraint);

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await CertificateAssertions.AssertCertificatePathLengthConstraintAsync(certName, vaultUri, credential, pathLengthConstraint);
   }

   /// <summary>
   /// Verifies that command-line validation fails when exportable is requested for HSM-backed key types.
   /// </summary>
   [Theory]
   [InlineData("RsaHsm", 2048, null)]
   [InlineData("EcHsm", null, "P256")]
   public async Task CreateCertificate_ExportableHsmKey_FailsValidation(string keyType, int? keySize, string? keyCurveName)
   {
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(
         certName: "test-cert",
         subjectName: SubjectName,
         expireMonths: ExpireMonths,
         vaultUri: new Uri("https://unit-test.vault.azure.net/"),
         keyType: keyType,
         exportable: true,
         keySize: keySize,
         keyCurveName: keyCurveName);

      var exitCode = await Program.Main(args);

      Assert.Equal(1, exitCode);
   }

   // ------------------------------------------------------------------
   // Helper
   // ------------------------------------------------------------------

}
