import asyncio
import base64
import io
from collections.abc import Callable

from azure.cognitiveservices.vision.computervision import ComputerVisionClient
from azure.cognitiveservices.vision.computervision.models import VisualFeatureTypes
from dapr.aio.clients import DaprClient
from msrest.authentication import CognitiveServicesCredentials

from app.config import Settings
from app.models import Category, FileResponse, NotificationMessage


class DaprGateway:
    """Dapr building blocks used by the service: service invocation, secrets and pub/sub."""

    def __init__(self, dapr: DaprClient, settings: Settings):
        self._dapr = dapr
        self._settings = settings

    async def get_secret(self, name: str) -> str:
        response = await self._dapr.get_secret(self._settings.secret_store, name)
        return response.secret[name]

    async def get_picture(self, file_reference: str) -> bytes:
        response = await self._dapr.invoke_method(
            self._settings.file_service_app_id,
            f"api/v1.0/File/{file_reference}",
            http_verb="GET",
        )
        return base64.b64decode(FileResponse.model_validate_json(response.data).base64)

    async def publish_notification(self, message: NotificationMessage) -> None:
        await self._dapr.publish_event(
            self._settings.pubsub_name,
            self._settings.notification_topic,
            data=message.model_dump_json(),
            data_content_type="application/json",
        )


def create_vision_client(endpoint: str, key: str) -> ComputerVisionClient:
    return ComputerVisionClient(endpoint, CognitiveServicesCredentials(key))


class AnalysisClient:
    """Azure Computer Vision image analysis."""

    def __init__(
        self,
        gateway: DaprGateway,
        settings: Settings,
        vision_client_factory: Callable[[str, str], ComputerVisionClient] = create_vision_client,
    ):
        self._gateway = gateway
        self._settings = settings
        self._vision_client_factory = vision_client_factory

    async def analyze_image(self, image: bytes) -> list[Category]:
        key = await self._gateway.get_secret(self._settings.cognitive_secret_name)
        client = self._vision_client_factory(self._settings.cognitive_service_url, key)

        # The Azure SDK is synchronous, keep the event loop free.
        result = await asyncio.to_thread(
            client.analyze_image_in_stream, io.BytesIO(image), visual_features=[VisualFeatureTypes.categories]
        )
        return [Category(name=c.name, score=c.score) for c in result.categories or []]
