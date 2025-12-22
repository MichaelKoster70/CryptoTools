# Usage
```
AzureDeleteSigningCert --CertificateName <name> --KeyVaultUri <uri> --TenantId <tenantId> --ClientId <clientId> [--ClientSecret <clientSecret> | --Interactive | --WorkloadIdentity]
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

      .\AzureDeleteCert --CertificateName "MySigningCA" --KeyVaultUri "https://my-key-vault.vault.azure.net/" --WorkloadIdentity
```
