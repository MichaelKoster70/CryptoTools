# Usage
```
AzureCreateSigningCert --Subject <subject> --CertificateName <name> --SignerCertificateName <rootName> --ExpireMonth <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> [--ClientSecret <clientSecret> | --Interactive | --WorkloadIdentity] [--Exportable] [--KeyType <keyType>] [--KeySize <keySize>] [--KeyCurveName <keyCurveName>]
```
or
```
AzureCreateSigningCert --Subject <subject> --FileName <name> [--Password <password>] --SignerCertificateName <rootName> --ExpireMonths <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> [--ClientSecret <clientSecret> | --Interactive | --WorkloadIdentity] [--Exportable] [--KeyType <keyType>] [--KeySize <keySize>] [--KeyCurveName <keyCurveName>]
```

Where:
* Subject: The subject of the certificate in form "CN=\<subject\>".
* CertificateName: The name of the certificate in Azure Key Vault.
* FileName: Absolute path to PFX file holding the certificate (<drive>:\<folder>\<name>.pfx)
* Password: The password to protect the private key contained in the PFX file, required with FileName option.
* SignerCertificateName: The name of the CA certificate in Azure Key Vault used for signing the leaf certificate.
* KeyVaultUri: The URI of the Azure Key Vault to store the certificate (like https://some-name.vault.azure.net/).
* TenantId: The Entra ID tenant ID.
* ClientId: The client ID of the service principal used to access the Key Vault.
* ClientSecret: The client secret of the service principal used to access the Key Vault.
* WorkloadIdentity: If set, the tool will use an Entra ID Managed Identity [Workload identity federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation) to access the Key Vault. Use this option when running the tool in an Azure Pipeline or a GitHub Action with workload identity federation configured.
* Interactive: If set, the tool will use interactive login to Entra ID to access the Key Vault.
* ExpireMonths: The number of months the certificate is valid, default is 1.
* Exportable: If set, the private key of the certificate will be marked as exportable, only applied for "Rsa" and "Ec" key types.
* KeyType: The type of key to use for the certificate. Valid values are "Rsa", "RsaHsm", "Ec", and "EcHsm".
* KeySize: The size of the RSA key, valid only if KeyType is "Rsa" or "RsaHsm".
* KeyCurveName: The name of the elliptic curve, valid only if KeyType is "Ec" or "EcHsm".

The tool will create the certificate in the supplied Azure Key Vault under the <CertificateName> name, signed by the <SignerCertificateName> certificate. The certificate will be created using:
* Private key marked as non exportable by default, or exportable if the Exportable option is set.
* Cipher Mode: RSA or EC depending on the KeyType option, default is RSA. RSA keys are created with 4096 Bit key size by default, and EC keys are created with P-384 curve by default.
* Signing: SHA384 for RSA keys; for EC keys: SHA256 (P-256/P-256K), SHA384 (P-384), SHA512 (P-521).
* The Key Types "RsaHsm" and "EcHsm" create the private key in an HSM backed Azure Key Vault (Premium SKU). Creation will fail if the Key Vault is not backed by an HSM.

Required permissions on Azure KeyVault:
- Sign with Key (Microsoft.KeyVault/vaults/keys/sign/action)
- Read Certificate Properties  (Microsoft.KeyVault/vaults/certificates/read)
- Create Certificate (Microsoft.KeyVault/vaults/certificates/create/action)

The WorkloadIdentity parameter relies on the [Azure Identity SDK for .NET](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.workloadidentitycredential?view=azure-dotnet) and requires the following environment variables to be set:
- AZURE_CLIENT_ID: The client ID of the Entra ID application representing the workload identity.
- AZURE_TENANT_ID: The tenant ID of the Entra ID tenant.
- AZURE_FEDERATED_TOKEN_FILE: The path to the file containing the OIDC token issued by the workload identity provider.

The following example shows how to use the tools in an Azure Pipeline.
```yaml
steps:
- task: AzureCLI@2
  inputs:
    azureSubscription: 'My-WIF-Service-Connection'  # Must be WIF-enabled
    addSpnToEnvironment: true
    scriptType: 'pscore'
    scriptLocation: 'inlineScript'
    inlineScript: |
      # write the OIDC JWT into a temp file
      $tokenPath = "$(Agent.TempDirectory)\federated-token.jwt"
      Set-Content -Path $tokenPath -Value $env:idToken

      # export the values the SDK needs
      $env:AZURE_CLIENT_ID            = $env:servicePrincipalId
      $env:AZURE_TENANT_ID            = $env:tenantId
      $env:AZURE_FEDERATED_TOKEN_FILE = $tokenPath

      .\AzureCreateSigningCert --Subject "CN=My Organization" --CertificateName "MySigningCA" --SignerCertificateName="MyRootCA" --ExpireMonths 2 --KeyVaultUri "https://my-key-vault.vault.azure.net/" --WorkloadIdentity
```
