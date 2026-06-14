// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace CertTools.AzureCertCore.Tests;

/// <summary>
/// Tests that validate parameter checking in <see cref="OptionsExtensions"/> and
/// the <see cref="Options"/> command-line model.
/// These tests do not require Azure connectivity.
/// </summary>
public class OptionsExtensionsTests
{
   // ------------------------------------------------------------------
   // ValidateKeyCreationOptions – invalid key type
   // ------------------------------------------------------------------

   [Theory]
   [InlineData("Unknown",  "P384", 4096)]
   [InlineData("",         "P384", 4096)]
   [InlineData("  ",       "P384", 4096)]
   public void ValidateKeyCreationOptions_InvalidKeyType_ReturnsFalse(string keyType, string keyCurveName, int keySize)
   {
      var result = OptionsExtensions.ValidateKeyCreationOptions(keyType, keyCurveName, keySize);
      Assert.False(result);
   }

   // ------------------------------------------------------------------
   // ValidateKeyCreationOptions – invalid RSA key size
   // ------------------------------------------------------------------

   [Theory]
   [InlineData("Rsa",    "P384", 1024)]
   [InlineData("Rsa",    "P384", 8192)]
   [InlineData("RsaHsm", "P384", 0)]
   [InlineData("RsaHsm", "P384", -1)]
   public void ValidateKeyCreationOptions_InvalidRsaKeySize_ReturnsFalse(string keyType, string keyCurveName, int keySize)
   {
      var result = OptionsExtensions.ValidateKeyCreationOptions(keyType, keyCurveName, keySize);
      Assert.False(result);
   }

   // ------------------------------------------------------------------
   // ValidateKeyCreationOptions – invalid EC curve name
   // ------------------------------------------------------------------

   [Theory]
   [InlineData("Ec",    "P192",    4096)]
   [InlineData("Ec",    "Unknown", 4096)]
   [InlineData("Ec",    "",        4096)]
   [InlineData("EcHsm", "Invalid", 4096)]
   public void ValidateKeyCreationOptions_InvalidEcKeyCurve_ReturnsFalse(string keyType, string keyCurveName, int keySize)
   {
      var result = OptionsExtensions.ValidateKeyCreationOptions(keyType, keyCurveName, keySize);
      Assert.False(result);
   }

   // ------------------------------------------------------------------
   // ValidateKeyCreationOptions – valid RSA options
   // ------------------------------------------------------------------

   [Theory]
   [InlineData("Rsa",    "P384", 2048)]
   [InlineData("Rsa",    "P384", 3072)]
   [InlineData("Rsa",    "P384", 4096)]
   [InlineData("RsaHsm", "P384", 2048)]
   [InlineData("RsaHsm", "P384", 3072)]
   [InlineData("RsaHsm", "P384", 4096)]
   public void ValidateKeyCreationOptions_ValidRsaOptions_ReturnsTrue(string keyType, string keyCurveName, int keySize)
   {
      var result = OptionsExtensions.ValidateKeyCreationOptions(keyType, keyCurveName, keySize);
      Assert.True(result);
   }

   // ------------------------------------------------------------------
   // ValidateKeyCreationOptions – valid EC options
   // ------------------------------------------------------------------

   [Theory]
   [InlineData("Ec",    "P256",  4096)]
   [InlineData("Ec",    "P256K", 4096)]
   [InlineData("Ec",    "P384",  4096)]
   [InlineData("Ec",    "P521",  4096)]
   [InlineData("EcHsm", "P256",  4096)]
   [InlineData("EcHsm", "P256K", 4096)]
   [InlineData("EcHsm", "P384",  4096)]
   [InlineData("EcHsm", "P521",  4096)]
   public void ValidateKeyCreationOptions_ValidEcOptions_ReturnsTrue(string keyType, string keyCurveName, int keySize)
   {
      var result = OptionsExtensions.ValidateKeyCreationOptions(keyType, keyCurveName, keySize);
      Assert.True(result);
   }

   [Theory]
   [InlineData("RsaHsm", "P384", 2048)]
   [InlineData("EcHsm", "P256", 4096)]
   public void ValidateKeyCreationOptions_HsmExportable_ReturnsFalse(string keyType, string keyCurveName, int keySize)
   {
      var result = OptionsExtensions.ValidateKeyCreationOptions(keyType, keyCurveName, keySize, exportable: true);

      Assert.False(result);
   }

   [Theory]
   [InlineData("Rsa", "P384", 4096)]
   [InlineData("Ec", "P384", 4096)]
   public void ValidateKeyCreationOptions_NonHsmExportable_ReturnsTrue(string keyType, string keyCurveName, int keySize)
   {
      var result = OptionsExtensions.ValidateKeyCreationOptions(keyType, keyCurveName, keySize, exportable: true);

      Assert.True(result);
   }

   // ------------------------------------------------------------------
   // ValidateKeyCreationOptions – case-insensitive matching
   // ------------------------------------------------------------------

   [Theory]
   [InlineData("rsa",   "p384", 4096)]
   [InlineData("RSA",   "P384", 4096)]
   [InlineData("ec",    "p256", 4096)]
   [InlineData("ECHSM", "P521", 4096)]
   public void ValidateKeyCreationOptions_CaseInsensitiveInput_ReturnsTrue(string keyType, string keyCurveName, int keySize)
   {
      var result = OptionsExtensions.ValidateKeyCreationOptions(keyType, keyCurveName, keySize);
      Assert.True(result);
   }

   // ------------------------------------------------------------------
   // GetKeyCreationOptions – correct object mapping from option strings
   // ------------------------------------------------------------------

   [Fact]
   public void GetKeyCreationOptions_RsaType_ReturnsRsaKeyCreationOptions()
   {
      var options = MakeCreateOptions(keyType: "Rsa", keySize: 4096, exportable: false, keyCurveName: "P384");

      var result = options.GetKeyCreationOptions();

      var rsaOptions = Assert.IsType<RsaKeyCreationOptions>(result);
      Assert.Equal(4096, rsaOptions.KeySize);
      Assert.False(rsaOptions.HsmBacked);
      Assert.False(rsaOptions.Exportable);
   }

   [Fact]
   public void GetKeyCreationOptions_RsaHsmType_ReturnsRsaKeyCreationOptionsHsmBacked()
   {
      var options = MakeCreateOptions(keyType: "RsaHsm", keySize: 3072, exportable: false, keyCurveName: "P384");

      var result = options.GetKeyCreationOptions();

      var rsaOptions = Assert.IsType<RsaKeyCreationOptions>(result);
      Assert.Equal(3072, rsaOptions.KeySize);
      Assert.True(rsaOptions.HsmBacked);
   }

   [Fact]
   public void Validate_HsmExportable_ReturnsNull()
   {
      var options = MakeCreateOptions(keyType: "RsaHsm", keySize: 3072, exportable: true, keyCurveName: "P384");

      var result = options.Validate();

      Assert.Null(result);
   }

   [Fact]
   public void GetKeyCreationOptions_EcType_ReturnsEcKeyCreationOptions()
   {
      var options = MakeCreateOptions(keyType: "Ec", keySize: 4096, exportable: false, keyCurveName: "P256");

      var result = options.GetKeyCreationOptions();

      var ecOptions = Assert.IsType<EcKeyCreationOptions>(result);
      Assert.Equal(EcKeyCurve.P256, ecOptions.KeyCurve);
      Assert.False(ecOptions.HsmBacked);
   }

   [Fact]
   public void GetKeyCreationOptions_EcHsmType_ReturnsEcKeyCreationOptionsHsmBacked()
   {
      var options = MakeCreateOptions(keyType: "EcHsm", keySize: 4096, exportable: false, keyCurveName: "P521");

      var result = options.GetKeyCreationOptions();

      var ecOptions = Assert.IsType<EcKeyCreationOptions>(result);
      Assert.Equal(EcKeyCurve.P521, ecOptions.KeyCurve);
      Assert.True(ecOptions.HsmBacked);
   }

   // ------------------------------------------------------------------
   // Validate<T> – credential validation
   // ------------------------------------------------------------------

   [Fact]
   public void Validate_InteractiveWithoutTenantId_ReturnsNull()
   {
      var options = MakeOptions(interactive: true, clientId: "some-client-id", tenantId: null, clientSecret: "", workloadIdentity: false);

      var result = options.Validate();

      Assert.Null(result);
   }

   [Fact]
   public void Validate_InteractiveWithoutClientId_ReturnsNull()
   {
      var options = MakeOptions(interactive: true, clientId: null, tenantId: "some-tenant-id", clientSecret: "", workloadIdentity: false);

      var result = options.Validate();

      Assert.Null(result);
   }

   [Fact]
   public void Validate_ClientSecretWithoutTenantId_ReturnsNull()
   {
      var options = MakeOptions(interactive: false, clientId: "some-client-id", tenantId: null, clientSecret: "secret", workloadIdentity: false);

      var result = options.Validate();

      Assert.Null(result);
   }

   [Fact]
   public void Validate_ClientSecretWithoutClientId_ReturnsNull()
   {
      var options = MakeOptions(interactive: false, clientId: null, tenantId: "some-tenant-id", clientSecret: "secret", workloadIdentity: false);

      var result = options.Validate();

      Assert.Null(result);
   }

   [Fact]
   public void Validate_WorkloadIdentity_ReturnsOptions()
   {
      var options = MakeOptions(interactive: false, clientId: null, tenantId: null, clientSecret: "", workloadIdentity: true);

      var result = options.Validate();

      Assert.NotNull(result);
   }

   // ------------------------------------------------------------------
   // Stub helpers
   // ------------------------------------------------------------------

   /// <summary>
   /// Creates a minimal concrete <see cref="OptionsBase"/> instance for credential-validation tests.
   /// </summary>
   private static ConcreteOptionsBase MakeOptions(
      bool interactive, string? clientId, string? tenantId, string clientSecret, bool workloadIdentity) =>
      new()
      {
         CertificateName = "test-cert",
         KeyVaultUri = "https://myvault.vault.azure.net/",
         Interactive = interactive,
         ClientId = clientId,
         TenantId = tenantId,
         ClientSecret = clientSecret,
         WorkloadIdentity = workloadIdentity
      };

   /// <summary>
   /// Creates a minimal concrete <see cref="OptionsCreateBase"/> instance for key-mapping tests.
   /// </summary>
   private static ConcreteOptionsCreateBase MakeCreateOptions(
      string keyType, int keySize, bool exportable, string keyCurveName) =>
      new()
      {
         CertificateName = "test-cert",
         KeyVaultUri = "https://myvault.vault.azure.net/",
         Interactive = false,
         ClientSecret = "",
         WorkloadIdentity = false,
         KeyType = keyType,
         KeySize = keySize,
         Exportable = exportable,
         KeyCurveName = keyCurveName
      };

   /// <summary>Minimal concrete subclass of <see cref="OptionsBase"/> used for unit tests.</summary>
   private sealed class ConcreteOptionsBase : OptionsBase;

   /// <summary>Minimal concrete subclass of <see cref="OptionsCreateBase"/> used for unit tests.</summary>
   private sealed class ConcreteOptionsCreateBase : OptionsCreateBase;
}
