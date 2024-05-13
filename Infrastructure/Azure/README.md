# Azure
Run the following command to create the necessary azure ressources.

```bash
az deployment group create --name <deployment Name> --resource-group <resource group> --template-file infrastructure.bicep --parameters @parameter.json
```