# Computervision
The *Computervision* service is responsible for processing the images and analyses the content.

## Process
```mermaid
    flowchart LR
        A[Topic] --> B[Computervision];
        B[Computervision] --> C[File Service];
        C[File Service] --> D[Blob Storage];
        B[Computervision] --> E[Azure Cognitive Services]
        B[Computervision] --> F[Topic]
```

## Debugging
The service can be easily debugged locallly.

**Requirements:**
- Dapr installed locally
- File Service running locally
- Azure Cognitive Services

In the _secrets.json_ set the _CognitiveServicesKey_ to the key of your Azure Cognitive Services account.
In the _secretstore.yaml_ replace the path to the _secrets.json_ file.
In the _lauchSettings.json_ set the COGNITIVE_SERVICE_URL_ to the url of your Azure Cognitive Services account.

Since the service utilizes _Man.Dapr.Sidekick.AspNetCore_ the application can be debugged locally by starting the application in debug mode.

## Deployment



