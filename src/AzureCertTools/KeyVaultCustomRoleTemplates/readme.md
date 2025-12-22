# Azure Key Vault Custom Role Templates
This folder contains custom role templates for Azure Key Vault.

## Policy Templates

| Template Name | Description |
| --- | --- |
| `SignWithCertificate.bicep` | Bicep template to create a custom role definition to allow signing with existing certificates. |
| `SignWithCertificate.json` | JSON template that holds the required permissions to sign with a Key Vault certificate. |
| `CreateAndSignCertificate.bicep` | Bicep template to create a custom role definition to allow creating and signing certificates. |
| `CreateAndSignCertificate.json` | JSON template that holds the required permissions to get and import certificates, and sign with a cert key. |
| `SelfSignedCertificatesOfficer.bicep` | Bicep template to create a custom role definition Key Vault Certificate Officer + signing with certificates. |
| `DeleteCertificate.bicep` | Bicep template to create a custom role definition to allow deleting certificates. |
| `DeleteCertificate.json` | JSON template to create a custom role definition to allow deleting certificates. |

## How to use the role templates
 - Open the policy file in a text editor, change the `assignableScopes`to the scope you want the role to be available
 - Use Azure PowerShell `Set-AzRoleDefinition`
 - Use Azure CLI `az role definition create --role-definition <file>`

or 
 - Deploy the Bicep file using Azure CLI `az deployment sub create --location <location> --template-file <file>`

## References
[Azure CLI: Custom Role](https://learn.microsoft.com/en-us/azure/role-based-access-control/tutorial-custom-role-cli)