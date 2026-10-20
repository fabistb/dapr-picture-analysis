@description('The container app environment name')
param containerAppEnvironmentName string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvironmentName
}

@description('The service bus namespace name')
param serviceBusNamespaceName string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

resource serviceBusAuthRule 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' existing = {
  parent: serviceBusNamespace
  name: 'RootManageSharedAccessKey'
}

resource messageBus 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: containerAppEnvironment
  name: 'messagebus'
  properties: {
    componentType: 'pubsub.azure.servicebus.topics'
    version: 'v1'
    secrets: [
      {
        name: 'servicebus-connection-string'
        value: serviceBusAuthRule.listKeys().primaryConnectionString
      }
    ]
    metadata: [
      {
        name: 'connectionString'
        secretRef: 'servicebus-connection-string'
      }
    ]
    scopes: [
      'gateway'
      'computervision'
      'notification'
    ]
  }
}
