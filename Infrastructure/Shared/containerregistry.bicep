@description('The container registry name')
param azureContainerRegistryName string

@description('The location')
param location string = resourceGroup().location

resource acrResource 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' ={
  name: azureContainerRegistryName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    adminUserEnabled: true
  }
}
