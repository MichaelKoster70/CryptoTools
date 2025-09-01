# CryptoTools
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

.NET 8 based crypto tools for
* Creating x.509 based code signing certificates for develoopment and testing purposes

## Overview

The current release supports the following features:
* CreateRootCert: Tool to create an X.509 root CA certificate
* CreateSigningCert: Tool to create an X.509 code signing certificate signed by the root CA certificate created withe the above tools
* AzureCreateRootCert: Tool to create an X.509 root CA certificate in Azure Key Vault
* AzureCreateSigningCert: Tool to create an X.509 code signing certificate signed by the root CA certificate in Azure Key Vault

## Usage

### CreateRootCert

```
CreateRootCert --Subject <subject> --Name <name> --Password <password> --ExpiryMonths <months>
```
Where:
* Subject: The subject of the certificate in form CN=\<subject\>.
* Name: The name of the certificate file (without extension).
* Password: The password to protect the private key contained in the certificate.
* ExpiryMonths: The number of months the certificate is valid, default is 240.

The tool will create a certificate file \<name\>.pfx in the current directory. The certificate file contains the private key and is protected by the password provided.
The generated certificate will be available in the certificate store of the current user under 'Personal'.
The generated certificate is self-signed using 4096 Bit RSA and SHA384.

### CreateSigningCert

```
CreateSigningCert --Subject <subject> --Name <name> --Password <password> --ExpireDays <days> --SignerThumbprint <thumbprint> 
```
or
```
CreateSigningCert --Subject <subject> --Name <name> --Password <password> --ExpireDays <days> --SignerPfx <pfxFile> --SignerPassword <store>
```

Where:
* Subject: The subject of the certificate in form CN=\<subject\>.
* Name: The name of the certificate file (ithout extension).
* Password: The password to protect the private key contained in the certificate.
* SignerThumbprint: the certificate thumbprint of the root CA certificate used to sign the code signing certificate. The thumbprint can be obtained from the certificate store.
* SignerPfx: the PFX file holding the root CA certificate used to sign the code signing certificate.
* SignerPassword: the password to open the PFX file holding the root CA certificate used to sign the code signing certificate.
* ExpireDays: The number of days the certificate is valid, default is 365.

### AzureCreateRootCert
```
AzureCreateRootCert --Subject <subject> --Name <name> --ExpireMonth <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> --ClientSecret <clientSecret>
```
or
```
AzureCreateRootCert --Subject <subject> --Name <name> --ExpireMonth <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> --Interactive
or
```
AzureCreateRootCert --Subject <subject> --Name <name> --ExpireMonth <months> --KeyVaultUri <uri> --WorkloadIdentity
```

Where:
* Subject: The subject of the certificate in form CN=\<subject\>.
* Name: The name of the certificate in Azure Key Vault.
* KeyVaultUri: The URI of the Azure Key Vault to store the certificate (like https://some-name.vault.azure.net/).
* TenantId: The Entra ID tenant ID.
* ClientId: The client ID of the service principal used to access the Key Vault.
* ClientSecret: The client secret of the service principal used to access the Key Vault.
* WorkloadIdentity: If set, the tool will use an Entra ID Managed Identity [Workload identity federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation) to access the Key Vault. Use this option when running the tool in an Azure Pipeline or a GitHub Action with workload identity federation configured.
* Interactive: If set, the tool will use interactive login to Entra ID to access the Key Vault.
* ExpiryMonths: The number of months the certificate is valid, default is 240.

Required permissions on Azure KeyVault:
- Sign with Key (Microsoft.KeyVault/vaults/keys/sign/action)
- Read Certificate Properties  (Microsoft.KeyVault/vaults/certificates/read)
- Create Certificate (Microsoft.KeyVault/vaults/certificates/create/action)

### AzureCreateSigningCert
```
AzureCreateSigningCert --Subject <subject> --CertificateName <name> --SignerCertificateName <rootName> --ExpireMonth <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> --ClientSecret <clientSecret>
```
or
```
AzureCreateSigningCert --Subject <subject> --CertificateName <name> --SignerCertificateName <rootName> --ExpireMonth <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> --Interactive
```
or
```
AzureCreateSigningCert --Subject <subject> --CertificateName <name> --SignerCertificateName <rootName> --ExpireMonth <months> --KeyVaultUri <uri> --WorkloadIdentity
```

Where:
* Subject: The subject of the certificate in form CN=\<subject\>.
* CertificateName: The name of the certificate in Azure Key Vault.
* SignerCertificateName: The name of the root CA certficate int Azure Key Vault used for signing the leaf certificate.
* KeyVaultUri: The URI of the Azure Key Vault to store the certificate (like https://some-name.vault.azure.net/).
* TenantId: The Entra ID tenant ID.
* ClientId: The client ID of the service principal used to access the Key Vault.
* ClientSecret: The client secret of the service principal used to access the Key Vault.
* WorkloadIdentity: If set, the tool will a Entra ID Managed Identity [Workload identity federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation) to access the Key Vault. Use this option when running the tool in an Azure Pipeline or an GitHub Action with workload identity federation configured.
* Interactive: If set, the tool will use interactive login to Entra ID to access the Key Vault.
* ExpiryMonths: The number of months the certificate is valid, default is 1.
 
Required permissions on Azure KeyVault:
- Sign with Key (Microsoft.KeyVault/vaults/keys/sign/action)
- Read Certificate Properties  (Microsoft.KeyVault/vaults/certificates/read)
- Create Certificate (Microsoft.KeyVault/vaults/certificates/create/action)

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
    scriptType: 'pscore'
    scriptLocation: 'inlineScript'
    inlineScript: |
      Write-Host "Using federated token from: $env:AZURE_FEDERATED_TOKEN_FILE"
      Write-Host "Client ID: $env:AZURE_CLIENT_ID"
      Write-Host "Tenant ID: $env:AZURE_TENANT_ID"

      .\AzureCreateRootCert --Subject "My Root CA" --Name "MyRootCA" --ExpireMonth 240 --KeyVaultUri "https://my-key-vault.vault.azure.net/" --WorkloadIdentity
```

## Getting Started

### Desktop PC prerequisites
You need a Windows based PC with:
- Window 10 x64 1809 or newer
- Visual Studio 2022 17.8 or newer with 
  - .NET 8 SDK installed
  - .NET 8 Runtime installed

### Build

1. Clone the repository
1. Open the solution in Visual Studio 2022 in [src](src) folder
1. Build the solution

## License
The tools are licensed under the [MIT license](LICENSE).

## References
- Azure Workload Identity Federation: https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation
