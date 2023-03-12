# This is a sample Python script.

# Press ⌃R to execute it or replace it with your code.
# Press Double ⇧ to search everywhere for classes, files, tool windows, actions, and settings.

import json
from array import array
import os
from PIL import Image
import sys
import time

from azure.cognitiveservices.vision.computervision import ComputerVisionClient
from msrest.authentication import CognitiveServicesCredentials
from cloudevents.sdk.event import v1
from dapr.clients import DaprClient
from dapr.clients.grpc._response import TopicEventResponse
from dapr.ext.grpc import App

app = App()


@app.subscribe(pubsub_name='messagebus', topic='message-received')
def message_topic(event: v1.Event) -> TopicEventResponse:
    data = json.loads(event.Data())

    file_resp = invoke_file_service(data)

    cognitiv_service_key = get_cognitiv_service_key()

    endpoint = ""
    computervision_client = ComputerVisionClient(endpoint, CognitiveServicesCredentials(cognitiv_service_key))
    tags_result = computervision_client.tag_image()

    return TopicEventResponse('success')


def get_cognitiv_service_key():
    with DaprClient() as d:
        key = 'azure-cognitiv-services-key'
        storeName = 'secretstore'

        secret_resp = d.get_secret(store_name=storeName, key=key)

        return secret_resp.secret


def invoke_file_service(data):
    with DaprClient() as daprClient:
        file_resp = daprClient.invoke_method(
            "file-service",
            f"api/v1.0/File/{data['fileReference']}",
            data=b'',
            http_verb="GET"
        )

        if file_resp.status_code != 200:
            raise Exception('invalid response code' + file_resp.status_code)

    return file_resp


app.run(50051)
