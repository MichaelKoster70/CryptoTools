// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------
// 
// Bicep module for the Azure Key Vault

@description('Name of the Key Vault to create.')
param name string

@description('The SKU name of the Key Vault (standard or premium).')
param skuName string

@description('Azure Region the Key Vaults will be created in.')
param location string = resourceGroup().location

@description('The objectId of the principal to assign the Owner Role')
param certificateOwnerUserPrincipalId string

@description('The objectId of the principal to assign the Crypto User Role')
param cryptoUserServicePrincipalId string

@description('Tags to apply to the Key Vault.')
param tags object

// Entra ID tenant ID
var tenantId = subscription().tenantId 

// Define role definition IDs for Key Vault roles
var keyVaultCertificatesOfficerRoleDefinitionId string = 'a4417e6f-fecd-4de8-b567-7b0420556985'
var keyVaultCryptoRoleDefinitionId string = '12338af0-0e69-4776-bea7-57ae8d297424'

// Create the Key Vault resource
// For now, keep it accessible from all networks, lock down later once a proper IP range update is in place
resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: name
  location: location
  properties: {
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableRbacAuthorization: true
    sku: {
      family: 'A'
      name: skuName
    }
    tenantId: tenantId
    accessPolicies: [] // not needed as we authorize using RBAC
  }
  tags: tags
}

// Certificate Officer Role for User
resource keyVaultRoleAssignmentCertificate 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('CertificateOfficer', keyVaultCertificatesOfficerRoleDefinitionId, keyVault.id)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultCertificatesOfficerRoleDefinitionId)
    principalId: certificateOwnerUserPrincipalId
    principalType: 'User'
  }
}

// Certificate Officer Role for Service Principal (needed to create certificates with private key operations enabled)
resource keyVaultRoleAssignmentCertificate2 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('CertificateOfficer', keyVaultCryptoRoleDefinitionId, keyVault.id)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultCertificatesOfficerRoleDefinitionId)
    principalId: cryptoUserServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Crypto User Role for Service Principal (needed to perform cryptographic operations)
resource keyVaultRoleAssignmentCrypto 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('CryptoUser', keyVaultCryptoRoleDefinitionId, keyVault.id)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultCryptoRoleDefinitionId)
    principalId: cryptoUserServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}
