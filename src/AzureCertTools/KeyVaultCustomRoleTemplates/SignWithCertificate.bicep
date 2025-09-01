// Azure Bicep template to Create a custom role definition to allow signing with existing certificates in Key Vault
targetScope = 'subscription'

var keyVaultSignWithCertificateRoleDefinitionId = guid(subscription().id, 'SignWithCertificate')

resource roleDefinitionSignWithCertificate 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
  name: keyVaultSignWithCertificateRoleDefinitionId
  properties: {
    roleName: 'Key Vault Sign with Certificate'
    description: 'Perform Key Vault x.509 certificate signing operations'
    type: 'CustomRole'
    permissions: [
      {
        actions: []
        notActions: []
        dataActions: [
          'Microsoft.KeyVault/vaults/keys/sign/action'
          'Microsoft.KeyVault/vaults/certificates/read'
        ]
        notDataActions: []
      }
    ]
    assignableScopes: [
      subscription().id
    ]
  }
}

output keyVaultSignWithCertificateRoleDefinitionId string = keyVaultSignWithCertificateRoleDefinitionId
