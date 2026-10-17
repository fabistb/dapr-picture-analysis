from functools import lru_cache
from typing import Annotated

from dapr.aio.clients import DaprClient
from dapr.ext.fastapi import DaprApp
from fastapi import Depends, FastAPI

from app.clients import AnalysisClient, DaprGateway
from app.config import Settings, get_settings
from app.models import CloudEvent
from app.service import ComputervisionService

app = FastAPI(title="Computervision")
dapr_app = DaprApp(app)

SettingsDep = Annotated[Settings, Depends(get_settings)]


@lru_cache
def get_dapr_client() -> DaprClient:
    return DaprClient()


def get_service(dapr: Annotated[DaprClient, Depends(get_dapr_client)], settings: SettingsDep) -> ComputervisionService:
    gateway = DaprGateway(dapr, settings)
    return ComputervisionService(gateway, AnalysisClient(gateway, settings), settings.cat_score_threshold)


ServiceDep = Annotated[ComputervisionService, Depends(get_service)]


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "Healthy"}


async def process_image(event: CloudEvent, service: ServiceDep) -> None:
    await service.process_image(event.data.file_reference)


# Subscribe on the same pub/sub component that is used for publishing notifications.
dapr_app.subscribe(pubsub=get_settings().pubsub_name, topic="message-received", route="/api/v1.0/computervision")(
    process_image
)
