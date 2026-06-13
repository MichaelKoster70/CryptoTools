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
   public static TheoryData<int, bool, string> RsaHsmKeyOptions()
   {
      var data = new TheoryData<int, bool, string>
      {
         { 2048, false, "rh2048ne" },
         { 2048, true, "rh2048ex" },
         { 3072, false, "rh3072ne" },
         { 3072, true, "rh3072ex" },
         { 4096, false, "rh4096ne" },
         { 4096, true, "rh4096ex" }
      };
      return data;
   }

   /// <summary>
   /// Verifies that an HSM-backed RSA root CA certificate is created in the Premium Key Vault
   /// for each supported RSA key size (2048, 3072, 4096 bits) and exportability setting.
   /// Requires: AZURE_KEYVAULT_URL_PREMIUM and a configured workload identity.
   /// </summary>
   [Theory]
   [MemberData(nameof(RsaHsmKeyOptions))]
   public async Task CreateCertificate_RsaHsmKey_Succeeds(int keySize, bool exportable, string prefix)
   {
      if (!TestConfiguration.HasPremiumWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, and AZURE_TENANT_ID must all be set and the runner must have a workload identity configured.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreatePremiumKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "RsaHsm", exportable, keySize: keySize);

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await AssertCertificatePropertiesAsync(certName, vaultUri, credential, CertificateKeyType.RsaHsm, exportable);
   }

   // ------------------------------------------------------------------
   // HSM-backed EC  (requires Premium Key Vault)
   // ------------------------------------------------------------------

   /// <summary>Provides EC HSM key curves supported by Azure Key Vault.</summary>
   public static TheoryData<string, bool, string> EcHsmKeyOptions()
   {
      var data = new TheoryData<string, bool, string>
      {
         { "P256", false, "ehp256ne" },
         { "P256", true, "ehp256ex" },
         { "P256K", false, "ehp256kne" },
         { "P256K", true, "ehp256kex" },
         { "P384", false, "ehp384ne" },
         { "P384", true, "ehp384ex" },
         { "P521", false, "ehp521ne" },
         { "P521", true, "ehp521ex" }
      };
      return data;
   }

   /// <summary>
   /// Verifies that an HSM-backed EC root CA certificate is created in the Premium Key Vault
   /// for each supported EC curve (P-256, P-256K, P-384, P-521) and exportability setting.
   /// Requires: AZURE_KEYVAULT_URL_PREMIUM and a configured workload identity.
   /// </summary>
   [Theory]
   [MemberData(nameof(EcHsmKeyOptions))]
   public async Task CreateCertificate_EcHsmKey_Succeeds(string keyCurveName, bool exportable, string prefix)
   {
      if (!TestConfiguration.HasPremiumWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, and AZURE_TENANT_ID must all be set and the runner must have a workload identity configured.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreatePremiumKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "EcHsm", exportable, keyCurveName: keyCurveName);

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await AssertCertificatePropertiesAsync(certName, vaultUri, credential, CertificateKeyType.EcHsm, exportable);
   }

   // ------------------------------------------------------------------
   // Software-backed RSA  (Standard Key Vault, 4096-bit only per issue)
   // ------------------------------------------------------------------

   /// <summary>
   /// Verifies that a software-backed RSA 4096-bit root CA certificate is created in the Standard Key Vault
   /// for both exportable and non-exportable key settings.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD and a configured workload identity.
   /// </summary>
   [Theory]
   [InlineData(false, "r4096ne")]
   [InlineData(true, "r4096ex")]
   public async Task CreateCertificate_RsaSoftwareKey_4096_Succeeds(bool exportable, string prefix)
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, and AZURE_TENANT_ID must all be set and the runner must have a workload identity configured.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "Rsa", exportable, keySize: 4096);

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await AssertCertificatePropertiesAsync(certName, vaultUri, credential, CertificateKeyType.Rsa, exportable);
   }

   // ------------------------------------------------------------------
   // Software-backed EC  (Standard Key Vault, P-384 per issue)
   // ------------------------------------------------------------------

   /// <summary>
   /// Verifies that a software-backed EC P-384 root CA certificate is created in the Standard Key Vault
   /// for both exportable and non-exportable key settings.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD and a configured workload identity.
   /// </summary>
   [Theory]
   [InlineData(false, "ep384ne")]
   [InlineData(true, "ep384ex")]
   public async Task CreateCertificate_EcSoftwareKey_P384_Succeeds(bool exportable, string prefix)
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, and AZURE_TENANT_ID must all be set and the runner must have a workload identity configured.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "Ec", exportable, keyCurveName: "P384");

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await AssertCertificatePropertiesAsync(certName, vaultUri, credential, CertificateKeyType.Ec, exportable);
   }

   /// <summary>
   /// Verifies that the root CA certificate contains or omits a path length constraint
   /// based on whether the command line argument is provided.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD and a configured workload identity.
   /// </summary>
   [Theory]
   [InlineData(null, "plnone")]
   [InlineData(1, "plone")]
   public async Task CreateCertificate_PathLengthConstraint_PresentAndAbsent_Succeeds(int? pathLengthConstraint, string prefix)
   {
      if (!TestConfiguration.HasWorkloadIdentityCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, and AZURE_TENANT_ID must all be set and the runner must have a workload identity configured.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.CreateStandardKeyVaultUri();
      var credential = fixture.CreateWorkloadIdentityCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);
      var args = CliArgumentBuilder.CreateWorkloadIdentityArgs(certName, SubjectName, ExpireMonths, vaultUri, keyType: "Rsa", exportable: false, keySize: 4096, pathLengthConstraint: pathLengthConstraint);

      var exitCode = await Program.Main(args);

      Assert.Equal(0, exitCode);
      await AssertCertificatePathLengthConstraintAsync(certName, vaultUri, credential, pathLengthConstraint);
   }

   // ------------------------------------------------------------------
   // Helper
   // ------------------------------------------------------------------

   private static async Task AssertCertificatePropertiesAsync(string certName, Uri vaultUri, Azure.Core.TokenCredential credential, CertificateKeyType expectedKeyType, bool expectedExportable)
   {
      var client = new CertificateClient(vaultUri, credential);
      var response = await client.GetCertificateAsync(certName);
      Assert.NotNull(response.Value);
      Assert.Equal(certName, response.Value.Name);
      Assert.Equal(expectedKeyType, response.Value.Policy.KeyType);
      Assert.Equal(expectedExportable, response.Value.Policy.Exportable);
   }

   private static async Task AssertCertificatePathLengthConstraintAsync(string certName, Uri vaultUri, Azure.Core.TokenCredential credential, int? expectedPathLengthConstraint)
   {
      var client = new CertificateClient(vaultUri, credential);
      var response = await client.GetCertificateAsync(certName);
      Assert.NotNull(response.Value);

      using var certificate = X509CertificateLoader.LoadCertificate(response.Value.Cer);
      var basicConstraints = certificate.Extensions
         .OfType<X509BasicConstraintsExtension>()
         .Single();

      Assert.True(basicConstraints.CertificateAuthority);

      if (expectedPathLengthConstraint.HasValue)
      {
         Assert.True(basicConstraints.HasPathLengthConstraint);
         Assert.Equal(expectedPathLengthConstraint.Value, basicConstraints.PathLengthConstraint);
      }
      else
      {
         Assert.False(basicConstraints.HasPathLengthConstraint);
      }
   }
}
