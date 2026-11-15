# Azure
Run the following command to create the necessary azure ressources.

```bash
az deployment group create --name <deployment Name> --resource-group <resource group> --template-file infrastructure.bicep --parameters @parameter.json
```

## Dapr Components
Dapr components deploy with `infrastructure.bicep`. Each component is a module in `DaprComponents/`:

| Module | Component | Type |
|---|---|---|
| `storage.bicep` | `file-entry-storage-binding` | `bindings.azure.blobstorage` |
| `notificationstorage.bicep` | `notification-storage` | `bindings.azure.blobstorage` |
| `messagebus.bicep` | `messagebus` | `pubsub.azure.servicebus.topics` |
| `secretstore.bicep` | `secretstore` | `secretstores.azure.keyvault` |
| `statestore.bicep` | `statestore` | `state.azure.cosmosdb` |

Components authenticate with the user-assigned managed identity (`azureClientId`); its roles are assigned by the modules in `RoleAssignments/` (`storage.bicep`, `servicebus.bicep`, `cosmosdb.bicep`). The Dapr components store no keys or component secrets.

The Cognitive Services key is still stored in Key Vault as `cognitive-service-key`. The Computervision services read it through the `secretstore` component.

### Upgrade existing environments
Earlier deployments stored the unused Key Vault secret `storage-connectionString`, which contains the storage account key. Bicep does not delete it on redeploy. After deploying this template, remove it once and rotate the key:

```bash
az keyvault secret delete --vault-name <key vault> --name storage-connectionString
# only needed if soft delete is active on the vault
az keyvault secret purge --vault-name <key vault> --name storage-connectionString
az storage account keys renew --resource-group <resource group> --account-name <storage account> --key primary
```

The Cosmos DB account, database (`daprstate`) and container (`statestore`, partition key `/partitionKey`) are created in `infrastructure.bicep`. The state store module gets the endpoint from the account.

No `az containerapp env dapr-component set` needed anymore.
