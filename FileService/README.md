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
 $ docker build -t fileservice  .
```