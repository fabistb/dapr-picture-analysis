FROM python:3.11-slim

WORKDIR /app

# Install uv
COPY --from=ghcr.io/astral-sh/uv:latest /uv /usr/local/bin/uv

# Copy project files
COPY pyproject.toml .
COPY main.py .

# Install dependencies using uv
RUN uv pip install --system --no-cache \
    dapr>=1.14.0 \
    dapr-ext-grpc>=1.14.0 \
    azure-cognitiveservices-vision-computervision>=0.9.0 \
    msrest>=0.7.1 \
    cloudevents>=1.11.0 \
    pillow>=10.4.0

# Set environment variables
ENV PYTHONUNBUFFERED=1

# Expose gRPC port
EXPOSE 50051

# Run the application
ENTRYPOINT ["python", "main.py"]
