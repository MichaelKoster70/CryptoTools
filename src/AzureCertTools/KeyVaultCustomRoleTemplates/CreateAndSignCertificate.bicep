// Azure Bicep template to Create a custom role definition to allow creating and signing certificates in Key Vault
targetScope = 'subscription'

var keyVaultCreateAndSignCertificateRoleDefinitionId = guid(subscription().id, 'CreateAndSignCertificate')

resource roleDefinitionCreateAndSignCertificate 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
  name: keyVaultCreateAndSignCertificateRoleDefinitionId
  properties: {
    roleName: 'Key Vault Create and Sign Certificate'
    description: 'Perform Key Vault x.509 certificate create and signing operations'
    type: 'CustomRole'
    permissions: [
      {
        actions: []
        notActions: []
        dataActions: [
          'Microsoft.KeyVault/vaults/keys/sign/action'
          'Microsoft.KeyVault/vaults/certificates/read'
          'Microsoft.KeyVault/vaults/certificates/create/action'
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
