# Notification

The **Notification** service is responsible for sending notifications to the user.

## Debugging
The service can be easily debugged locally.

**Requirements:**
- Dapr installed locally

```
$ dapr run --app-id notification --app-protocol http --app-port 8080 --dapr-http-port 3500 --dapr-grpc-port 50001 --components-path ./components
```

## Deployment