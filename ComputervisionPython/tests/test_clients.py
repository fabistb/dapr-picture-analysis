import base64
import json
from types import SimpleNamespace

from app.clients import AnalysisClient, DaprGateway
from app.config import Settings
from app.models import NotificationMessage

settings = Settings(cognitive_service_url="https://vision.test")


class FakeDapr:
    def __init__(self):
        self.published = []
        self.invoked = []

    async def get_secret(self, store, key):
        return SimpleNamespace(secret={key: f"{store}-secret"})

    async def invoke_method(self, app_id, method, http_verb):
        self.invoked.append((app_id, method, http_verb))
        return SimpleNamespace(data=json.dumps({"base64": base64.b64encode(b"img").decode()}).encode())

    async def publish_event(self, pubsub, topic, data, data_content_type):
        self.published.append((pubsub, topic, json.loads(data), data_content_type))


async def test_gateway_get_picture():
    dapr = FakeDapr()

    assert await DaprGateway(dapr, settings).get_picture("abc") == b"img"
    assert dapr.invoked == [("file-service", "api/v1.0/File/abc", "GET")]


async def test_gateway_publish_notification():
    dapr = FakeDapr()

    await DaprGateway(dapr, settings).publish_notification(NotificationMessage(message="hi"))

    assert dapr.published == [("messagebus", "notification-received", {"message": "hi"}, "application/json")]


async def test_analysis_client_maps_categories():
    seen = {}

    class FakeVision:
        def analyze_image_in_stream(self, stream, visual_features):
            seen["image"] = stream.read()
            return SimpleNamespace(categories=[SimpleNamespace(name="animal_cat", score=0.8)])

    def factory(endpoint, key):
        seen["endpoint"], seen["key"] = endpoint, key
        return FakeVision()

    client = AnalysisClient(DaprGateway(FakeDapr(), settings), settings, factory)

    categories = await client.analyze_image(b"img")

    assert [(c.name, c.score) for c in categories] == [("animal_cat", 0.8)]
    assert seen == {"image": b"img", "endpoint": "https://vision.test", "key": "secretstore-secret"}


async def test_analysis_client_handles_no_categories():
    class FakeVision:
        def analyze_image_in_stream(self, stream, visual_features):
            return SimpleNamespace(categories=None)

    client = AnalysisClient(DaprGateway(FakeDapr(), settings), settings, lambda e, k: FakeVision())

    assert await client.analyze_image(b"img") == []
