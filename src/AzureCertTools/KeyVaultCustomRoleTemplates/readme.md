# Azure Key Vault Custom Role Templates
This folder contains custom role templates for Azure Key Vault.

## Policy Templates

| Template Name | Description |
| --- | --- |
| `SignWithCertificate.json` | This template holds the required permissions to sign with a Key Vault certificate. |
| `CreateAndSignCertificate.json` | This template holds the required permissions to get and import certificates, and sign with a cert key |

## How to use the role templates
  - Open the policy file in a text editor, change the `assignableScopes`to the scope you want the role to be available
 - Use Azure PowerShell `Set-AzRoleDefinition`
 - Use Azure CLI `az role definition create --role-definition <file>`

## References
[Azure CLI: Custom Role](https://learn.microsoft.com/en-us/azure/role-based-access-control/tutorial-custom-role-cli)