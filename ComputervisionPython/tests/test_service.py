import pytest

from app.models import Category, NotificationMessage
from app.service import ComputervisionService


class FakeDapr:
    def __init__(self):
        self.published: list[NotificationMessage] = []

    async def get_picture(self, file_reference: str) -> bytes:
        return b"image"

    async def publish_notification(self, message: NotificationMessage) -> None:
        self.published.append(message)


class FakeAnalysis:
    def __init__(self, categories: list[Category]):
        self._categories = categories

    async def analyze_image(self, image: bytes) -> list[Category]:
        return self._categories


async def run(categories: list[Category]) -> tuple[bool, FakeDapr]:
    dapr = FakeDapr()
    return await ComputervisionService(dapr, FakeAnalysis(categories)).process_image("ref"), dapr


async def test_cat_above_threshold_publishes_notification():
    notified, dapr = await run([Category(name="animal_Cat", score=0.9)])

    assert notified
    assert dapr.published == [NotificationMessage(message="The submitted picture probably contains a cat")]


@pytest.mark.parametrize(
    "categories",
    [
        [Category(name="animal_cat", score=0.5)],
        [Category(name="animal_dog", score=0.99)],
        [],
    ],
)
async def test_no_notification(categories):
    notified, dapr = await run(categories)

    assert not notified
    assert dapr.published == []
