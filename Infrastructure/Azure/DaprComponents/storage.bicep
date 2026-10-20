@description('The container app environment name')
param containerAppEnvironmentName string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvironmentName
}

@description('The storage account name')
param storageAccountName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
}

resource fileEntryBinding 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: containerAppEnvironment
  name: 'file-entry-storage-binding'
  properties: {
    componentType: 'bindings.azure.blobstorage'
    version: 'v1'
    secrets: [
      {
        name: 'storage-account-key'
        value: storageAccount.listKeys().keys[0].value
      }
    ]
    metadata: [
      {
        name: 'accountName'
        value: storageAccountName
      }
      {
        name: 'accountKey'
        secretRef: 'storage-account-key'
      }
      {
        name: 'containerName'
        value: 'file-entry'
      }
      {
        name: 'decodeBase64'
        value: 'true'
      }
    ]
    scopes: [
      'file-service'
    ]
  }
}
