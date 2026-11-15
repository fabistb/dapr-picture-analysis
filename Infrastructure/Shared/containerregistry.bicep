@description('The container registry name')
param azureContainerRegistryName string

@description('The location')
param location string = resourceGroup().location

resource acrResource 'Microsoft.ContainerRegistry/registries@2025-11-01' ={
  name: azureContainerRegistryName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    adminUserEnabled: true
  }
}
