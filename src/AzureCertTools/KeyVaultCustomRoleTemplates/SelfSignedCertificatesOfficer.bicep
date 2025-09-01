// Azure Bicep template to Create  a custom role definition to allow creating and signing self-signed certificates in Key Vault
targetScope = 'subscription'

var keyVaultSelfSignedCertificatesOfficerRoleDefinitionId = guid(subscription().id, 'KeyVaultSelfSignedCertificatesOfficer')

resource roleDefinitionKeyVaultSelfSignedCertificatesOfficer 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
  name: keyVaultSelfSignedCertificatesOfficerRoleDefinitionId
  properties: {
    roleName: 'Key Vault Self Signed Certificates Officer'
    description: 'Perform any action on the certificates of a key vault, except manage permissions.'
    type: 'CustomRole'
    permissions: [
      {
        actions: [
          'Microsoft.Authorization/*/read'
          'Microsoft.Insights/alertRules/*'
          'Microsoft.Resources/deployments/*'
          'Microsoft.Resources/subscriptions/resourceGroups/read'
          'Microsoft.Support/*'
          'Microsoft.KeyVault/checkNameAvailability/read'
          'Microsoft.KeyVault/deletedVaults/read'
          'Microsoft.KeyVault/locations/*/read'
          'Microsoft.KeyVault/vaults/*/read'
          'Microsoft.KeyVault/operations/read'
        ]
        notActions: []
        dataActions: [
          'Microsoft.KeyVault/vaults/certificatecas/*'
          'Microsoft.KeyVault/vaults/certificates/*'
          'Microsoft.KeyVault/vaults/certificatecontacts/write'
          'Microsoft.KeyVault/vaults/keys/sign/action'
        ]
        notDataActions: []
      }
    ]
    assignableScopes: [
      subscription().id
    ]
  }
}

output keyVaultSelfSignedCertificatesOfficerRoleDefinitionId string = keyVaultSelfSignedCertificatesOfficerRoleDefinitionId