@description('The container app environment name')
param containerAppEnvironmentName string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2026-07-01' existing = {
  name: containerAppEnvironmentName
}

@description('The service bus namespace name')
param serviceBusNamespaceName string

@description('The client id of the user managed identity used to access the Azure resource')
param managedIdentityClientId string

resource messageBus 'Microsoft.App/managedEnvironments/daprComponents@2026-07-01' = {
  parent: containerAppEnvironment
  name: 'messagebus'
  properties: {
    componentType: 'pubsub.azure.servicebus.topics'
    version: 'v1'
    metadata: [
      {
        name: 'namespaceName'
        value: '${serviceBusNamespaceName}.servicebus.windows.net'
      }
      {
        name: 'azureClientId'
        value: managedIdentityClientId
      }
    ]
    scopes: [
      'gateway'
      'computervision'
      'notification'
    ]
  }
}
