@description('The container app environment name')
param containerAppEnvironmentName string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvironmentName
}

@description('The storage account name')
param storageAccountName string

@description('The client id of the user managed identity used to access the Azure resource')
param managedIdentityClientId string

resource notificationBinding 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: containerAppEnvironment
  name: 'notification-storage'
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
        value: 'notification-storage'
      }
      {
        name: 'azureClientId'
        value: managedIdentityClientId
      }
    ]
    scopes: [
      'notification'
    ]
  }
}
