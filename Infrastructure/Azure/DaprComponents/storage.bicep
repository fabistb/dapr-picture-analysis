@description('The container app environment name')
param containerAppEnvironmentName string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvironmentName
}

@description('The storage account name')
param storageAccountName string

@description('The client id of the user managed identity used to access the Azure resource')
param managedIdentityClientId string

resource fileEntryBinding 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: containerAppEnvironment
  name: 'file-entry-storage-binding'
  properties: {
    componentType: 'bindings.azure.blobstorage'
    version: 'v1'
    metadata: [
      {
        name: 'accountName'
        value: storageAccountName
      }
      {
        name: 'containerName'
        value: 'file-entry'
      }
      {
        name: 'decodeBase64'
        value: 'true'
      }
      {
        name: 'azureClientId'
        value: managedIdentityClientId
      }
    ]
    scopes: [
      'file-service'
    ]
  }
}
