# syntax=docker/dockerfile:1

FROM golang:1.27-alpine AS build

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY *.go ./
RUN CGO_ENABLED=0 GOOS=linux go build -trimpath -ldflags="-s -w" -o /notification

# Runs as root like the .NET images: port 80 is privileged, and a non-root
# user cannot bind it on runtimes without ip_unprivileged_port_start=0.
FROM gcr.io/distroless/static-debian12

COPY --from=build /notification /notification

EXPOSE 80

ENTRYPOINT [ "/notification" ]
