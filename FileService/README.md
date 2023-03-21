# File Service

The *File Service* is responsible for storing files in the *BlobStorage* and providing other services access to these files.

## Debugging
The service can be easily debugged locally.

**Requirements:**
- Dapr installed locally
- *BlobStorage* Account

To start the service replace the account details in the _blobstorage.yaml_ file in the components folder.

Since the service utilizes _Man.Dapr.Sidekick.AspNetCore_ the application can be debugged locally by starting the application in debug mode.

Example requests can be found in the _requests_ folder.

## Deployment
To create a docker image for the service navigate to the _FileService_ folder and run the following command:

```bash
 $ docker build --platform amd64 -t fileservice  .
```

## Push Image to Container Registry
To push the image to the container registry run the following command:

```bash
 $ az acr login --name <container registry>
 $ docker tag fileservice <container registry>/fileservice
 $ docker push <container registry>/fileservice
```

## Deploy to Azure Container Apps
Deploy the _FileService_ service to azure container app.

```bash
az containerapp create \
  --name fileservice \
  --resource-group <resource group> \
  --environment dapr-picture-cae-001 \
  --image <container registry>/fileservice:latest \
  --target-port 80 \
  --ingress 'internal' \
  --min-replicas 1 \
  --max-replicas 1 \
  --enable-dapr \
  --dapr-app-id file-service \
  --dapr-app-port 80 \
  --registry-server <container registry> \
  --user-assigned <managed identity>
```