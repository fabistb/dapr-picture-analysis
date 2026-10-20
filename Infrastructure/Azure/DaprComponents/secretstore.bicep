@description('The container app environment name')
param containerAppEnvironmentName string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvironmentName
}

@description('The key vault name')
param keyVaultName string

@description('The client id of the user managed identity used to access the key vault')
param managedIdentityClientId string

resource secretStore 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: containerAppEnvironment
  name: 'secretstore'
  properties: {
    componentType: 'secretstores.azure.keyvault'
    version: 'v1'
    metadata: [
      {
        name: 'vaultName'
        value: keyVaultName
      }
      {
        name: 'azureEnvironment'
        value: 'AZUREPUBLICCLOUD'
      }
      {
        name: 'azureClientId'
        value: managedIdentityClientId
      }
    ]
    scopes: [
      'gateway'
      'file-service'
      'computervision'
      'notification'
    ]
  }
}
