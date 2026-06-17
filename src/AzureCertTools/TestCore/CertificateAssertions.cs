// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace CertTools.TestCore;

/// <summary>
/// Provides shared assertions for Azure Key Vault certificate integration tests.
/// </summary>
public static class CertificateAssertions
{
   /// <summary>
   /// Verifies that the certificate exists and matches the expected key type and exportable setting.
   /// </summary>
   /// <param name="certificateName">Azure Key Vault certificate name.</param>
   /// <param name="vaultUri">URI of the Key Vault that contains the certificate.</param>
   /// <param name="credential">Credential used to access the Key Vault.</param>
   /// <param name="expectedKeyType">Expected certificate key type.</param>
   /// <param name="expectedExportable">Expected exportable setting.</param>
   public static async Task AssertCertificatePropertiesAsync(
      string certificateName,
      Uri vaultUri,
      TokenCredential credential,
      CertificateKeyType expectedKeyType,
      bool expectedExportable)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(certificateName);
      ArgumentNullException.ThrowIfNull(vaultUri);
      ArgumentNullException.ThrowIfNull(credential);

      var client = new CertificateClient(vaultUri, credential);
      var response = await client.GetCertificateAsync(certificateName);

      Assert.NotNull(response.Value);
      Assert.Equal(certificateName, response.Value.Name);
      Assert.Equal(expectedKeyType, response.Value.Policy.KeyType);
      Assert.Equal(expectedExportable, response.Value.Policy.Exportable);
   }

   /// <summary>
   /// Verifies that the certificate contains or omits a path length constraint as expected.
   /// </summary>
   /// <param name="certificateName">Azure Key Vault certificate name.</param>
   /// <param name="vaultUri">URI of the Key Vault that contains the certificate.</param>
   /// <param name="credential">Credential used to access the Key Vault.</param>
   /// <param name="expectedPathLengthConstraint">Expected path length constraint, or <see langword="null"/> when no constraint should be present.</param>
   public static async Task AssertCertificatePathLengthConstraintAsync(
      string certificateName,
      Uri vaultUri,
      TokenCredential credential,
      int? expectedPathLengthConstraint)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(certificateName);
      ArgumentNullException.ThrowIfNull(vaultUri);
      ArgumentNullException.ThrowIfNull(credential);

      var client = new CertificateClient(vaultUri, credential);
      var response = await client.GetCertificateAsync(certificateName);

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
