# Notification Dotnet
The dotnet implementation of the _Notification_ service.
The service is responsible for sending notifications to the user.

## Debugging
The service can be easily debugged locally.

**Requirements:**
- Dapr installed locally
- File Service running locally

Since the service utilizes _Man.Dapr.Sidekick.AspNetCore_ the appication can be debugged locally by starting the application in debug mode.

## Deployment
To create a docker image for the service navigate to the _NotificationDotnet_ folder and run the following command:

```bash
 $ docker build --platform amd64 -t notificationdotnet  .
```

## Push Image to Container Registry
To push the image to the container registry run the following command:

```bash
 $ az acr login --name <container registry>
 $ docker tag notificationdotnet <container registry>/notificationdotnet
 $ docker push <container registry>/notificationdotnet
```

## Deploy to Azure Container Apps
Deploy the _Notification_ service to azure container app.

```bash
az containerapp create \
  --name notification \
  --resource-group <resource group> \
  --environment <container app environment> \
  --image <container registry>/notificationdotnet:latest \
  --target-port 80 \
  --ingress 'internal' \
  --min-replicas 1 \
  --max-replicas 1 \
  --enable-dapr \
  --dapr-app-id notification \
  --dapr-app-port 80 \
  --registry-server <container registry> \
  --user-assigned <managed identity>
  ```