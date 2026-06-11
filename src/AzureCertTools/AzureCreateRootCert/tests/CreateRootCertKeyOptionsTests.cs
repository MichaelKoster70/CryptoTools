// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Security.KeyVault.Certificates;
using CertTools.AzureCertCore;

namespace CertTools.AzureCreateRootCert.Tests;

/// <summary>
/// Integration tests verifying that <see cref="CertificateWorker"/> correctly creates root CA certificates
/// for every supported key type, key size, and EC curve combination.
/// </summary>
[Collection("KeyVault")]
public class CreateRootCertKeyOptionsTests(KeyVaultFixture fixture)
{
   private const string SubjectName = "CN=Integration Test Root CA";
   private const int ExpireMonths = 12;

   // ------------------------------------------------------------------
   // HSM-backed RSA  (requires Premium Key Vault)
   // ------------------------------------------------------------------

   /// <summary>Provides RSA HSM key sizes supported by Azure Key Vault.</summary>
   public static IEnumerable<object[]> RsaHsmKeyOptions()
   {
      yield return [new RsaKeyCreationOptions { KeySize = 2048, HsmBacked = true }, "rh2048"];
      yield return [new RsaKeyCreationOptions { KeySize = 3072, HsmBacked = true }, "rh3072"];
      yield return [new RsaKeyCreationOptions { KeySize = 4096, HsmBacked = true }, "rh4096"];
   }

   /// <summary>
   /// Verifies that an HSM-backed RSA root CA certificate is created in the Premium Key Vault
   /// for each supported RSA key size (2048, 3072, 4096 bits).
   /// Requires: AZURE_KEYVAULT_URL_PREMIUM and ClientSecret credentials.
   /// </summary>
   [Theory]
   [MemberData(nameof(RsaHsmKeyOptions))]
   public async Task CreateRootCertAsync_RsaHsmKey_Succeeds(RsaKeyCreationOptions keyOptions, string prefix)
   {
      if (!TestConfiguration.HasPremiumKeyVaultCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_CLIENT_SECRET must all be set.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.GetPremiumKeyVaultUri();
      var credential = fixture.GetClientSecretCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);

      var result = await CertificateWorker.CreateRootCertAsync(
         certName, SubjectName, ExpireMonths, pathLengthConstraint: null, vaultUri, credential, keyOptions);

      Assert.Equal(certName, result);
      await AssertCertificateKeyTypeAsync(certName, vaultUri, credential, CertificateKeyType.RsaHsm);
   }

   // ------------------------------------------------------------------
   // HSM-backed EC  (requires Premium Key Vault)
   // ------------------------------------------------------------------

   /// <summary>Provides EC HSM key curves supported by Azure Key Vault.</summary>
   public static IEnumerable<object[]> EcHsmKeyOptions()
   {
      yield return [new EcKeyCreationOptions { KeyCurve = EcKeyCurve.P256,  HsmBacked = true }, "ehp256"];
      yield return [new EcKeyCreationOptions { KeyCurve = EcKeyCurve.P256K, HsmBacked = true }, "ehp256k"];
      yield return [new EcKeyCreationOptions { KeyCurve = EcKeyCurve.P384,  HsmBacked = true }, "ehp384"];
      yield return [new EcKeyCreationOptions { KeyCurve = EcKeyCurve.P521,  HsmBacked = true }, "ehp521"];
   }

   /// <summary>
   /// Verifies that an HSM-backed EC root CA certificate is created in the Premium Key Vault
   /// for each supported EC curve (P-256, P-256K, P-384, P-521).
   /// Requires: AZURE_KEYVAULT_URL_PREMIUM and ClientSecret credentials.
   /// </summary>
   [Theory]
   [MemberData(nameof(EcHsmKeyOptions))]
   public async Task CreateRootCertAsync_EcHsmKey_Succeeds(EcKeyCreationOptions keyOptions, string prefix)
   {
      if (!TestConfiguration.HasPremiumKeyVaultCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_PREMIUM, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_CLIENT_SECRET must all be set.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName(prefix);
      var vaultUri = fixture.GetPremiumKeyVaultUri();
      var credential = fixture.GetClientSecretCredential();
      fixture.RegisterForCleanup(certName, vaultUri, credential);

      var result = await CertificateWorker.CreateRootCertAsync(
         certName, SubjectName, ExpireMonths, pathLengthConstraint: null, vaultUri, credential, keyOptions);

      Assert.Equal(certName, result);
      await AssertCertificateKeyTypeAsync(certName, vaultUri, credential, CertificateKeyType.EcHsm);
   }

   // ------------------------------------------------------------------
   // Software-backed RSA  (Standard Key Vault, 4096-bit only per issue)
   // ------------------------------------------------------------------

   /// <summary>
   /// Verifies that a software-backed RSA 4096-bit root CA certificate is created in the Standard Key Vault.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD and ClientSecret credentials.
   /// </summary>
   [Fact]
   public async Task CreateRootCertAsync_RsaSoftwareKey_4096_Succeeds()
   {
      if (!TestConfiguration.HasClientSecretCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_CLIENT_SECRET must all be set.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName("r4096");
      var vaultUri = fixture.GetStandardKeyVaultUri();
      var credential = fixture.GetClientSecretCredential();
      var keyOptions = new RsaKeyCreationOptions { KeySize = 4096, HsmBacked = false };
      fixture.RegisterForCleanup(certName, vaultUri, credential);

      var result = await CertificateWorker.CreateRootCertAsync(
         certName, SubjectName, ExpireMonths, pathLengthConstraint: null, vaultUri, credential, keyOptions);

      Assert.Equal(certName, result);
      await AssertCertificateKeyTypeAsync(certName, vaultUri, credential, CertificateKeyType.Rsa);
   }

   // ------------------------------------------------------------------
   // Software-backed EC  (Standard Key Vault, P-384 per issue)
   // ------------------------------------------------------------------

   /// <summary>
   /// Verifies that a software-backed EC P-384 root CA certificate is created in the Standard Key Vault.
   /// Requires: AZURE_KEYVAULT_URL_STANDARD and ClientSecret credentials.
   /// </summary>
   [Fact]
   public async Task CreateRootCertAsync_EcSoftwareKey_P384_Succeeds()
   {
      if (!TestConfiguration.HasClientSecretCredentials)
      {
         Assert.Skip("AZURE_KEYVAULT_URL_STANDARD, AZURE_CLIENT_ID, AZURE_TENANT_ID, and AZURE_CLIENT_SECRET must all be set.");
      }

      var certName = KeyVaultFixture.GenerateCertificateName("ep384");
      var vaultUri = fixture.GetStandardKeyVaultUri();
      var credential = fixture.GetClientSecretCredential();
      var keyOptions = new EcKeyCreationOptions { KeyCurve = EcKeyCurve.P384, HsmBacked = false };
      fixture.RegisterForCleanup(certName, vaultUri, credential);

      var result = await CertificateWorker.CreateRootCertAsync(
         certName, SubjectName, ExpireMonths, pathLengthConstraint: null, vaultUri, credential, keyOptions);

      Assert.Equal(certName, result);
      await AssertCertificateKeyTypeAsync(certName, vaultUri, credential, CertificateKeyType.Ec);
   }

   // ------------------------------------------------------------------
   // Helper
   // ------------------------------------------------------------------

   private static async Task AssertCertificateKeyTypeAsync(
      string certName, Uri vaultUri, Azure.Core.TokenCredential credential, CertificateKeyType expectedKeyType)
   {
      var client = new CertificateClient(vaultUri, credential);
      var response = await client.GetCertificateAsync(certName);
      Assert.NotNull(response.Value);
      Assert.Equal(certName, response.Value.Name);
      Assert.Equal(expectedKeyType, response.Value.Policy.KeyType);
   }
}
