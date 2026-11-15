@description('The service bus namespace name')
param serviceBusNamespaceName string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

@description('The principal id of the user managed identity')
param principalId string

// Data Owner, the dapr topics pubsub creates subscriptions
var serviceBusDataOwner = '090c5cfd-751d-490a-894a-3ce6f1109419'

resource serviceBusRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBusNamespace
  name: guid(serviceBusNamespace.id, principalId, serviceBusDataOwner)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataOwner)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
