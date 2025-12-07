# syntax=docker/dockerfile:1

# Build stage
FROM golang:1.24 AS builder

WORKDIR /app

# Download Go modules
COPY go.mod go.sum ./
RUN go mod download

# Copy the source code
COPY *.go ./

# Build
RUN CGO_ENABLED=0 GOOS=linux go build -o /notification

# Runtime stage - use minimal distroless image
FROM gcr.io/distroless/static-debian12:nonroot

WORKDIR /

# Copy the binary from builder
COPY --from=builder /notification /notification

# Expose the application port
EXPOSE 8080

# Run as non-root user
USER nonroot:nonroot

# Run the application
ENTRYPOINT ["/notification"]