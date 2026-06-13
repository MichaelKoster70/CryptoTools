// Azure Bicep template for creating a resource group in Azure.
targetScope = 'subscription'

@description('Name of the resource group to create.')
param name string

@description('Azure Region the resource group will be created in.')
param location string = deployment().location

@description('Tags to apply to the resource group.')
param tags object = {
  Environment: 'Test'
}

// Creating the resource group, and applying a lock to prevent deletion.
resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: name
  location: location
  tags: tags
}

module applyLock 'resource-group-applylock.bicep' = {
  scope: resourceGroup
  name: 'applyLock'
  params: {
  }
}

