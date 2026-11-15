@description('The container app environment name')
param containerAppEnvironmentName string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2026-07-01' existing = {
  name: containerAppEnvironmentName
}

@description('The Cosmos DB account name')
param cosmosDbAccountName string

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2026-03-15' existing = {
  name: cosmosDbAccountName
}

@description('The Cosmos DB database name')
param cosmosDbDatabase string

@description('The Cosmos DB collection name')
param cosmosDbCollection string

@description('The client id of the user managed identity used to access the Azure resource')
param managedIdentityClientId string

resource stateStore 'Microsoft.App/managedEnvironments/daprComponents@2026-07-01' = {
  parent: containerAppEnvironment
  name: 'statestore'
  properties: {
    componentType: 'state.azure.cosmosdb'
    version: 'v1'
    metadata: [
      {
        name: 'url'
        value: cosmosDbAccount.properties.documentEndpoint
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
      {
        name: 'azureClientId'
        value: managedIdentityClientId
      }
    ]
  }
}
