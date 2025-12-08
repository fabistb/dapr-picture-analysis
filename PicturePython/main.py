"""
PicturePython - Python implementation of the Computervision service
"""
import json
import os
import base64
from io import BytesIO
from typing import List, Any

from azure.cognitiveservices.vision.computervision import ComputerVisionClient
from azure.cognitiveservices.vision.computervision.models import VisualFeatureTypes
from msrest.authentication import CognitiveServicesCredentials
from cloudevents.sdk.event import v1
from dapr.clients import DaprClient
from dapr.clients.grpc._response import TopicEventResponse
from dapr.ext.grpc import App

app = App()

# Configuration constants
CAT_DETECTION_CONFIDENCE_THRESHOLD = 0.5


class FileDao:
    """Data access object for file operations"""
    
    @staticmethod
    def get_picture(file_reference: str) -> str:
        """Fetch picture from file service"""
        with DaprClient() as dapr_client:
            response = dapr_client.invoke_method(
                "file-service",
                f"api/v1.0/File/{file_reference}",
                data=b'',
                http_verb="GET"
            )
            
            if response.status_code != 200:
                raise Exception(f'Invalid response code: {response.status_code}')
            
            file_response = json.loads(response.data)
            return file_response['base64']


class AnalysisService:
    """Service for analyzing images using Azure Cognitive Services"""
    
    @staticmethod
    def get_cognitive_service_key() -> str:
        """Retrieve cognitive service key from Dapr secret store"""
        with DaprClient() as dapr_client:
            secret_resp = dapr_client.get_secret(
                store_name='secretstore',
                key='cognitive-service-key'
            )
            return secret_resp.secret['cognitive-service-key']
    
    @staticmethod
    def analyze_image(base64_image: str) -> List[Any]:
        """Analyze image using Azure Cognitive Services"""
        cognitive_service_key = AnalysisService.get_cognitive_service_key()
        endpoint = os.getenv('COGNITIVE_SERVICE_URL', '')
        
        if not endpoint:
            raise Exception('COGNITIVE_SERVICE_URL environment variable is not set')
        
        client = ComputerVisionClient(
            endpoint,
            CognitiveServicesCredentials(cognitive_service_key)
        )
        
        # Convert base64 to stream
        image_data = base64.b64decode(base64_image)
        image_stream = BytesIO(image_data)
        
        # Analyze image with categories
        visual_features = [VisualFeatureTypes.categories]
        analysis_result = client.analyze_image_in_stream(
            image_stream,
            visual_features=visual_features
        )
        
        return analysis_result.categories


class ComputervisionService:
    """Main service for processing images"""
    
    @staticmethod
    def process_image(file_reference: str):
        """Process image and publish notification if cat is detected"""
        # Get picture from file service
        base64_image = FileDao.get_picture(file_reference)
        
        # Analyze image
        categories = AnalysisService.analyze_image(base64_image)
        
        # Check for cat in categories
        cat_category = None
        for category in categories:
            if 'cat' in category.name.lower():
                cat_category = category
                break
        
        # Publish notification if cat detected with high confidence
        if cat_category and cat_category.score > CAT_DETECTION_CONFIDENCE_THRESHOLD:
            with DaprClient() as dapr_client:
                notification_message = {
                    'message': 'The submitted picture probably contains a cat'
                }
                dapr_client.publish_event(
                    pubsub_name='messagebus',
                    topic_name='notification-received',
                    data=json.dumps(notification_message),
                    data_content_type='application/json'
                )


@app.subscribe(pubsub_name='messagebus', topic='message-received')
def message_topic(event: v1.Event) -> TopicEventResponse:
    """Handle incoming messages from messagebus"""
    try:
        data = json.loads(event.Data())
        file_reference = data.get('fileReference')
        
        if not file_reference:
            return TopicEventResponse('drop')
        
        ComputervisionService.process_image(file_reference)
        return TopicEventResponse('success')
    
    except Exception as e:
        print(f"Error processing image: {str(e)}")
        return TopicEventResponse('retry')


if __name__ == '__main__':
    app.run(50051)
