// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace CertTools.AzureCreateSslServerCert.Tests;

public class ProgramTests
{
   [Fact]
   public async Task Main_InvalidHsmExportableCombination_ReturnsError()
   {
      string[] args =
      [
         "--CertificateName", "test-cert",
         "--FQDN", "example.test",
         "--SignerCertificateName", "root-ca",
         "--KeyVaultUri", "https://unit-test.vault.azure.net/",
         "--WorkloadIdentity",
         "--KeyType", "EcHsm",
         "--KeyCurveName", "P256",
         "--Exportable"
      ];

      var exitCode = await Program.Main(args);

      Assert.Equal(1, exitCode);
   }
}
