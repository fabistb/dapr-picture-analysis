from functools import lru_cache

from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    cognitive_service_url: str = ""
    cognitive_secret_name: str = "cognitive-service-key"
    secret_store: str = "secretstore"
    pubsub_name: str = "messagebus"
    file_service_app_id: str = "file-service"
    notification_topic: str = "notification-received"
    cat_score_threshold: float = 0.5


@lru_cache
def get_settings() -> Settings:
    return Settings()
