# syntax=docker/dockerfile:1

FROM python:3.13-slim AS build

COPY --from=ghcr.io/astral-sh/uv:latest /uv /usr/local/bin/uv

ENV UV_COMPILE_BYTECODE=1 UV_LINK_MODE=copy UV_PYTHON_DOWNLOADS=never

WORKDIR /app

COPY pyproject.toml uv.lock ./
RUN --mount=type=cache,target=/root/.cache/uv uv sync --frozen --no-install-project

COPY app ./app
COPY tests ./tests

# Fail the image build if any unit test fails
RUN uv run --frozen pytest -q

# Drop dev dependencies for the runtime image
RUN --mount=type=cache,target=/root/.cache/uv uv sync --frozen --no-install-project --no-dev

FROM python:3.13-slim AS final

WORKDIR /app

# Runs as root like the other images: port 80 is privileged.
COPY --from=build /app/.venv ./.venv
COPY --from=build /app/app ./app

ENV PATH="/app/.venv/bin:$PATH" PYTHONUNBUFFERED=1

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80"]
