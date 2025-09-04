### Usage
```
AzureCreateSigningCert --Subject <subject> --CertificateName <name> --SignerCertificateName <rootName> --ExpireMonth <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> --ClientSecret <clientSecret>
```
or
```
AzureCreateSigningCert --Subject <subject> --CertificateName <name> --SignerCertificateName <rootName> --ExpireMonth <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> --Interactive
```
or
```
AzureCreateSigningCert --Subject <subject> --CertificateName <name> --SignerCertificateName <rootName> --ExpireMonth <months> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> --WorkloadIdentity
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
