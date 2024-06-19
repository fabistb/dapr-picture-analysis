# Extenders
Extenders allow to add resources which radius currently doesn't suppor.

## Push recipe to Azure Container Registry
```bash
rad bicep publish --file blobstoragecontainerdapr.bicep --target br:<container registry login server>/blobstoragecontainerdapr:latest
```

## Add extender to rad environment
```bash
rad recipe register blobstoragecontainerdapr --resource-type "Applications.Core/extenders" --template-kind bicep --template-path <container registry login server>/blobstoragecontainerdapr:latest
```