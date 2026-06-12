// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------
//
// Bicep Script to create all resources needed for the GitHub Actions based development and release workflow including
// * Resource Group for Testing environments with a lock to prevent deletion
// * Azure KeyVaults for testing secrets and certificates

targetScope = 'subscription'

@description('Azure Region where the resources will be created in.')
param location string = deployment().location

@description('Environment for the deployment')
param environment string = 'Test'

@description('The objectId of the principal to assign the Owner Role')
param certificateOwnerUserPrincipalId string

@description('The objectId of the principal to assign the Crypto User Role')
param cryptoUserServicePrincipalId string

// Azure resource names
var resourceGroupName string = 'rg-dev-certtools-test-weu'
var keyVaultStandardName string = 'kv-s-dev-test-weu'
var keyVaultPremiumName string = 'kv-p-dev-test-weu'

var tags = {
  Environment: environment
}

module rgModule 'resource-group.bicep' = {
  name: 'dev-certtools-ResourceGroup'
  scope: subscription()
  params: {
    name: resourceGroupName
    location: location
    tags: tags
  }
}

module keyVaultStandardModule 'key-vault.bicep' = {
  name: 'dev-certtools-KeyVault-Standard'
  scope: resourceGroup(resourceGroupName)
  params: {
    skuName: 'standard'
    name: keyVaultStandardName
    location: location
    tags: tags
    certificateOwnerUserPrincipalId: certificateOwnerUserPrincipalId
    cryptoUserServicePrincipalId: cryptoUserServicePrincipalId
  }
  dependsOn: [
    rgModule
  ]
}

module keyVaultPremiumModule 'key-vault.bicep' = {
  name: 'dev-certtools-KeyVault-Premium'
  scope: resourceGroup(resourceGroupName)
  params: {
    skuName: 'premium'
    name: keyVaultPremiumName
    location: location
    tags: tags
    certificateOwnerUserPrincipalId: certificateOwnerUserPrincipalId
    cryptoUserServicePrincipalId: cryptoUserServicePrincipalId
  }
  dependsOn: [
    rgModule
  ]
}
