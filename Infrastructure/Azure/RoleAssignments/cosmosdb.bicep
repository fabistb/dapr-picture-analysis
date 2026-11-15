@description('The Cosmos DB account name')
param cosmosDbAccountName string

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' existing = {
  name: cosmosDbAccountName
}

@description('The principal id of the user managed identity')
param principalId string

// Built-in Cosmos DB Data Contributor
var cosmosDbDataContributor = '00000000-0000-0000-0000-000000000002'

resource cosmosDbRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-11-15' = {
  parent: cosmosDbAccount
  name: guid(cosmosDbAccount.id, principalId, cosmosDbDataContributor)
  properties: {
    roleDefinitionId: '${cosmosDbAccount.id}/sqlRoleDefinitions/${cosmosDbDataContributor}'
    principalId: principalId
    scope: cosmosDbAccount.id
  }
}
