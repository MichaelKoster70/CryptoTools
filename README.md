# CryptoTools
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

.NET 10 based crypto tools for
* Creating x.509 based code signing certificates for development and testing purposes
* Managing x.509 certificates in Azure Key Vault

## Overview

The current release supports the following features:
* CreateRootCert: Tool to create an X.509 root CA certificate
* CreateSigningCert: Tool to create an X.509 code signing certificate signed by the root CA certificate created withe the above tools
* AzureCreateRootCert: Tool to create an X.509 root CA certificate in Azure Key Vault
* AzureCreateIntermediateCert: Tool to create an X.509 intermediate CA certificate in Azure Key Vault
* AzureCreateSigningCert: Tool to create an X.509 code signing certificate signed by the root CA certificate in Azure Key Vault

## Usage

### CreateRootCert

```
CreateRootCert --Subject <subject> --Name <name> --Password <password> --ExpiryMonths <months>
```
Where:
* Subject: The subject of the certificate in form "CN=\<subject\>".
* Name: The name of the certificate file (without extension).
* Password: The password to protect the private key contained in the certificate.
* ExpiryMonths: The number of months the certificate is valid, default is 240.

The tool will create a certificate file \<name\>.pfx in the current directory. The certificate file contains the private key and is protected by the password provided.
The generated certificate will be available in the certificate store of the current user under 'Personal'.
The generated certificate is self-signed using 4096 Bit RSA and SHA384.

### CreateSigningCert

```
CreateSigningCert --Subject <subject> --Name <name> --Password <password> --ExpireMonths <months> --SignerThumbprint <thumbprint> 
```
or
```
CreateSigningCert --Subject <subject> --Name <name> --Password <password> --ExpireMonths <months> --SignerPfx <pfxFile> --SignerPassword <store>
```

Where:
* Subject: The subject of the certificate in form "CN=\<subject\>".
* Name: The name of the certificate file (without extension).
* Password: The password to protect the private key contained in the certificate.
* SignerThumbprint: the certificate thumbprint of the root CA certificate used to sign the code signing certificate. The thumbprint can be obtained from the certificate store.
* SignerPfx: the PFX file holding the root CA certificate used to sign the code signing certificate.
* SignerPassword: the password to open the PFX file holding the root CA certificate used to sign the code signing certificate.
* ExpireMonths: The number of months the certificate is valid, default is 12.

### AzureCreateRootCert
```
AzureCreateRootCert --Subject <subject> --CertificateName <name> --ExpireMonths <months> [--PathLengthConstraint <length> ] --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> [--ClientSecret <clientSecret> | --Interactive | --WorkloadIdentity] [--Exportable] [--KeyType <keyType>] [--KeySize <keySize> | --KeyCurveName <curveName>]
```

Where:
* Subject: The subject of the certificate in form "CN=\<subject\>".
* CertificateName: The name of the certificate in Azure Key Vault.
* KeyVaultUri: The URI of the Azure Key Vault to store the certificate (like https://some-name.vault.azure.net/).
* PathLengthConstraint: If specified, the generated CA certificate will have a path length constraint extension with the provided length. This limits the maximum number of intermediate CA certificates that can be created under this root CA certificate. If not specified, no path length constraint will be set.
* TenantId: The Entra ID tenant ID.
* ClientId: The client ID of the service principal used to access the Key Vault.
* ClientSecret: The client secret of the service principal used to access the Key Vault.
* WorkloadIdentity: If set, the tool will use an Entra ID Managed Identity [Workload identity federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation) to access the Key Vault. Use this option when running the tool in an Azure Pipeline or a GitHub Action with workload identity federation configured.
* Interactive: If set, the tool will use interactive login to Entra ID to access the Key Vault.
* ExpireMonths: The number of months the certificate is valid, default is 240.
* Exportable: If set, the private key of the certificate will be marked as exportable, only applied for "Rsa" and "Ec" key types.
* KeyType: The type of key to use for the certificate. Valid values are "Rsa", "RsaHsm", "Ec", and "EcHsm".
* KeySize: The size of the RSA key, valid only if KeyType is "Rsa" or "RsaHsm".
* KeyCurveName: The name of the elliptic curve, valid only if KeyType is "Ec" or "EcHsm".

The tool will create a the certificate in the supplied Azure Key Vault under the <CertificateName> name. The certificate will be created using:
* Private key marked as non exportable by default, or exportable if the Exportable option is set.
* Cipher Mode: RSA or EC depending on the KeyType option, default is RSA. RSA keys are created with 4096 Bit key size by default, and EC keys are created with P-384 curve by default.
* Signing: SHA384 for RSA keys; for EC keys: SHA256 (P-256/P-256K), SHA384 (P-384), SHA512 (P-521).
* The Key Types "RsaHsm" and "EcHsm" create the private key in an HSM backed Azure Key Vault (Premium SKU). Creation will fail if the Key Vault is not backed by an HSM.

Required permissions on Azure KeyVault:
- Sign with Key (Microsoft.KeyVault/vaults/keys/sign/action)
- Read Certificate Properties  (Microsoft.KeyVault/vaults/certificates/read)
- Create Certificate (Microsoft.KeyVault/vaults/certificates/create/action)

### AzureCreateIntermediateCert
```
AzureCreateIntermediateCert --Subject <subject> --CertificateName <name> --SignerCertificateName <rootName> --ExpireMonths <months> [--PathLengthConstraint <length> ]--KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> [--ClientSecret <clientSecret> | --Interactive | --WorkloadIdentity] [--Exportable] [--KeyType <keyType>] [--KeySize <keySize> | --KeyCurveName <curveName>] 
```

Where:
* Subject: The subject of the certificate in form "CN=\<subject\>".
* CertificateName: The name of the certificate in Azure Key Vault.
* SignerCertificateName: The name of the CA certificate in Azure Key Vault used for signing the leaf certificate.
* KeyVaultUri: The URI of the Azure Key Vault to store the certificate (like https://some-name.vault.azure.net/).
* PathLengthConstraint: If specified, the generated CA certificate will have a path length constraint extension with the provided length. This limits the maximum number of intermediate CA certificates that can be created under this root CA certificate. If not specified, no path length constraint will be set.
* TenantId: The Entra ID tenant ID.
* ClientId: The client ID of the service principal used to access the Key Vault.
* ClientSecret: The client secret of the service principal used to access the Key Vault.
* WorkloadIdentity: If set, the tool will use an Entra ID Managed Identity [Workload identity federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation) to access the Key Vault. Use this option when running the tool in an Azure Pipeline or a GitHub Action with workload identity federation configured.
* Interactive: If set, the tool will use interactive login to Entra ID to access the Key Vault.
* ExpireMonths: The number of months the certificate is valid, default is 240.
* Exportable: If set, the private key of the certificate will be marked as exportable, only applied for "Rsa" and "Ec" key types.
* KeyType: The type of key to use for the certificate. Valid values are "Rsa", "RsaHsm", "Ec", and "EcHsm".
* KeySize: The size of the RSA key, valid only if KeyType is "Rsa" or "RsaHsm".
* KeyCurveName: The name of the elliptic curve, valid only if KeyType is "Ec" or "EcHsm".

The tool will create the certificate in the supplied Azure Key Vault under the <CertificateName> name, signed by the <SignerCertificateName> certificate. The certificate will be created using:
* Private key marked as non exportable by default, or exportable if the Exportable option is set.
* Cipher Mode: RSA or EC depending on the KeyType option, default is RSA. RSA keys are created with 4096 Bit key size by default, and EC keys are created with P-384 curve by default.
* Signing: SHA384 for RSA keys, and appropriate curves for EC keys.
* The Key Types "RsaHsm" and "EcHsm" create the private key in an HSM backed Azure Key Vault (Premium SKU). Creation will fail if the Key Vault is not backed by an HSM.

Required permissions on Azure KeyVault:
- Sign with Key (Microsoft.KeyVault/vaults/keys/sign/action)
- Read Certificate Properties  (Microsoft.KeyVault/vaults/certificates/read)
- Create Certificate (Microsoft.KeyVault/vaults/certificates/create/action)


### AzureCreateSigningCert
```
AzureCreateSigningCert --Subject <subject> --CertificateName <name> --SignerCertificateName <rootName> --ExpireMonths <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> [--ClientSecret <clientSecret> | --Interactive | --WorkloadIdentity] [--Exportable] [--KeyType <keyType>] [--KeySize <keySize> | --KeyCurveName <curveName>]
```
or
```
AzureCreateSigningCert --Subject <subject> --FileName <name> --SignerCertificateName <rootName> --ExpireMonths <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> [--ClientSecret <clientSecret> | --Interactive | --WorkloadIdentity] [--Exportable] [--KeyType <keyType>] [--KeySize <keySize> | --KeyCurveName <curveName>]
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

## AzureDeleteCert
```
AzureDeleteCert --CertificateName <name> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> [--ClientSecret <clientSecret> | --Interactive | --WorkloadIdentity]
```

Where:
* CertificateName: The name of the certificate in Azure Key Vault.
* KeyVaultUri: The URI of the Azure Key Vault to store the certificate (like https://some-name.vault.azure.net/).
* TenantId: The Entra ID tenant ID.
* ClientId: The client ID of the service principal used to access the Key Vault.
* ClientSecret: The client secret of the service principal used to access the Key Vault.
* WorkloadIdentity: If set, the tool will a Entra ID Managed Identity [Workload identity federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation) to access the Key Vault. Use this option when running the tool in an Azure Pipeline or an GitHub Action with workload identity federation configured.
* Interactive: If set, the tool will use interactive login to Entra ID to access the Key Vault.
 
Required permissions on Azure KeyVault:
- Read Certificate Properties  (Microsoft.KeyVault/vaults/certificates/read)
- Delete Certificate (Microsoft.KeyVault/vaults/certificates/delete)

## Workload Identity Federation
The WorkloadIdentity parameter relies on the [Azure Identity SDK for .NET](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.workloadidentitycredential?view=azure-dotnet) and requires the following environment variables to be set:
- AZURE_CLIENT_ID: The client ID of the Entra ID application representing the workload identity.
- AZURE_TENANT_ID: The tenant ID of the Entra ID tenant.
- AZURE_FEDERATED_TOKEN_FILE: The path to the file containing the OIDC token issued by the workload identity provider.

## Azure Pipelines usage
The tools can be used in Azure Pipelines with [Azure Resource Manager (ARM) service connection](https://learn.microsoft.com/en-us/azure/devops/pipelines/library/connect-to-azure?view=azure-devops) with Workload identity federation configured. 

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

      .\AzureCreateRootCert --Subject "My Root CA" --CertificateName "MyRootCA" --ExpireMonths 240 --KeyVaultUri "https://my-key-vault.vault.azure.net/" --WorkloadIdentity
```

## Getting Started

### Desktop PC prerequisites
You need a Windows based PC with:
- Windows 11 x64 24H2 or newer
- Visual Studio 2026 18.0 or newer with
  - .NET 10 SDK installed

### Build
1. Clone the repository
2. Open the solution in Visual Studio 2026in [src](src) folder
3. Build the solution

## License
The tools are licensed under the [MIT license](LICENSE).

## References
- Azure Workload Identity Federation: https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation
