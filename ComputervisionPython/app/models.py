from pydantic import AliasChoices, BaseModel, ConfigDict, Field


class Message(BaseModel):
    """Payload of a message-received event."""

    model_config = ConfigDict(populate_by_name=True)

    file_reference: str = Field(validation_alias=AliasChoices("fileReference", "FileReference", "file_reference"))


class CloudEvent(BaseModel):
    """CloudEvent envelope as delivered by the Dapr sidecar."""

    model_config = ConfigDict(extra="allow")

    data: Message


class Category(BaseModel):
    name: str
    score: float


class NotificationMessage(BaseModel):
    message: str


class FileResponse(BaseModel):
    base64: str
