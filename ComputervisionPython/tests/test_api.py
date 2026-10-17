import pytest
from fastapi.testclient import TestClient

from app.main import app, get_service


class FakeService:
    def __init__(self):
        self.processed: list[str] = []

    async def process_image(self, file_reference: str) -> bool:
        self.processed.append(file_reference)
        return True


@pytest.fixture
def service():
    return FakeService()


@pytest.fixture
def client(service):
    app.dependency_overrides[get_service] = lambda: service
    yield TestClient(app)
    app.dependency_overrides.clear()


def test_health(client):
    assert client.get("/health").json() == {"status": "Healthy"}


def test_subscribe(client):
    subscriptions = client.get("/dapr/subscribe").json()

    assert len(subscriptions) == 1
    assert subscriptions[0]["pubsubname"] == "messagebus"
    assert subscriptions[0]["topic"] == "message-received"
    assert subscriptions[0]["route"] == "/api/v1.0/computervision"


@pytest.mark.parametrize("key", ["fileReference", "FileReference"])
def test_event_is_processed(client, service, key):
    response = client.post("/api/v1.0/computervision", json={"data": {key: "abc"}})

    assert response.status_code == 200
    assert service.processed == ["abc"]


def test_invalid_event_returns_422(client, service):
    assert client.post("/api/v1.0/computervision", json={"data": {}}).status_code == 422
    assert service.processed == []
