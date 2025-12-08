FROM python:3.11-slim

WORKDIR /app

# Install uv (modern Python package manager)
COPY --from=ghcr.io/astral-sh/uv:latest /uv /usr/local/bin/uv

# Copy project files
COPY pyproject.toml .
COPY main.py .

# Update CA certificates
RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates && rm -rf /var/lib/apt/lists/*

# Install dependencies
# Ideally use: RUN uv pip install --system --no-cache --native-tls .
# However, for compatibility with various Docker build environments, we use pip directly
# Dependencies are still maintained in pyproject.toml for local development with uv
RUN pip install --no-cache-dir \
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
