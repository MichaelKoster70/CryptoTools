// Azure Bicep template to Create a custom role definition to allow creating and signing certificates in Key Vault
targetScope = 'subscription'

var keyVaultDeleteCertificateRoleDefinitionId = guid(subscription().id, 'DeleteCertificate')

resource roleDefinitionDeleteCertificate 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
  name: keyVaultDeleteCertificateRoleDefinitionId
  properties: {
    roleName: 'Key Vault Delete Certificate'
    description: 'Perform Key Vault x.509 certificate delete operations'
    type: 'CustomRole'
    permissions: [
      {
        actions: []
        notActions: []
        dataActions: [
          'Microsoft.KeyVault/vaults/certificates/read'
          'Microsoft.KeyVault/vaults/certificates/delete'
        ]
        notDataActions: []
      }
    ]
    assignableScopes: [
      subscription().id
    ]
  }
}

output keyVaultCreateAndSignCertificateRoleDefinitionId string = keyVaultCreateAndSignCertificateRoleDefinitionId
