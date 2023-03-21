# Notification
The _Notification_ service is responsible for sending notifications to the user.

**Important:**
The go implementation of the _Notification_ service currently has a problem processing multiple requests after another.
This leads to panics when the application runs in the Azure Container App.

## Debugging
The service can be easily debugged locally.

**Requirements:**
- Dapr installed locally

```
$ dapr run --app-id notification --app-protocol http --app-port 8080 --dapr-http-port 3500 --dapr-grpc-port 50001 --components-path ./components
```

## Deployment
To create a docker image for the service navigate to the _Notification_ folder and run the following command:

```bash
 $ docker build --platform amd64 -t notification  .
```

## Push Image to Container Registry
To push the image to the container registry run the following command:

```bash
 $ az acr login --name <container registry>
 $ docker tag notification <container registry>/notification
 $ docker push <container registry>/notification
```

## Deploy to Azure Container Apps
Deploy the _Notification_ service to azure container app.

```bash
az containerapp create \
  --name notification \
  --resource-group <resource group> \
  --environment <container app environment> \
  --image <container registry>/notification:latest \
  --target-port 8080 \
  --ingress 'internal' \
  --min-replicas 1 \
  --max-replicas 1 \
  --enable-dapr \
  --dapr-app-id notification \
  --dapr-app-port 8080 \
  --registry-server <container registry> \
  --user-assigned <managed identity>
  ```