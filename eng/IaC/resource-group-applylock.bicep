targetScope = 'resourceGroup'

resource dontDeleteLock 'Microsoft.Authorization/locks@2020-05-01' = {
  name: 'DoNotDelete'
  properties: {
    level: 'CanNotDelete'
    notes: 'Prevent deletion of the resourceGroup'
  }
}
