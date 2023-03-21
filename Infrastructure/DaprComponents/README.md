# Dapr Components
Dapr components contain all dapr related components to deploy to the Azure Container Apps.

# Blob Storage Binding

```bash
az containerapp env dapr-component set \
    --name <container app environment> \
    --resource-group <resource group> \
    --dapr-component-name file-entry-storage-binding \
    --yaml storage.yaml
```

```bash
az containerapp env dapr-component set \
    --name <container app environment> \
    --resource-group <resource group> \
    --dapr-component-name notification-storage \
    --yaml notificationstorage.yaml
```

# Azure Servicebus PubSub Topic

```bash
az containerapp env dapr-component set \
    --name <container app environment> \
    --resource-group <resource group> \
    --dapr-component-name messagebus \
    --yaml messagebus.yaml
```

# Azure Secret Store

```bash
az containerapp env dapr-component set \
    --name <container app environment> \
    --resource-group <resource group> \
    --dapr-component-name secretstore \
    --yaml secretstore.yaml
```