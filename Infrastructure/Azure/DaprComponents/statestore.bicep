@description('The container app environment name')
param containerAppEnvironmentName string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvironmentName
}

@description('The Cosmos DB account name')
param cosmosDbAccountName string

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' existing = {
  name: cosmosDbAccountName
}

@description('The Cosmos DB database name')
param cosmosDbDatabase string

@description('The Cosmos DB collection name')
param cosmosDbCollection string

resource stateStore 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: containerAppEnvironment
  name: 'statestore'
  properties: {
    componentType: 'state.azure.cosmosdb'
    version: 'v1'
    secrets: [
      {
        name: 'cosmosdb-master-key'
        value: cosmosDbAccount.listKeys().primaryMasterKey
      }
    ]
    metadata: [
      {
        name: 'url'
        value: cosmosDbAccount.properties.documentEndpoint
      }
      {
        name: 'masterKey'
        secretRef: 'cosmosdb-master-key'
      }
      {
        name: 'database'
        value: cosmosDbDatabase
      }
      {
        name: 'collection'
        value: cosmosDbCollection
      }
      {
        name: 'actorStateStore'
        value: 'true'
      }
    ]
  }
}
