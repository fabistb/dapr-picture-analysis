from typing import Protocol

from app.models import Category, NotificationMessage


class Dapr(Protocol):
    async def get_picture(self, file_reference: str) -> bytes: ...
    async def publish_notification(self, message: NotificationMessage) -> None: ...


class Analysis(Protocol):
    async def analyze_image(self, image: bytes) -> list[Category]: ...


class ComputervisionService:
    def __init__(self, dapr: Dapr, analysis: Analysis, cat_score_threshold: float = 0.5):
        self._dapr = dapr
        self._analysis = analysis
        self._threshold = cat_score_threshold

    async def process_image(self, file_reference: str) -> bool:
        """Analyse the picture and notify if it probably shows a cat. Returns True if notified."""
        image = await self._dapr.get_picture(file_reference)
        categories = await self._analysis.analyze_image(image)

        cat = next((c for c in categories if "cat" in c.name.lower()), None)
        if cat is None or cat.score <= self._threshold:
            return False

        await self._dapr.publish_notification(NotificationMessage(message="The submitted picture probably contains a cat"))
        return True
