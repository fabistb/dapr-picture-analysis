# syntax=docker/dockerfile:1

FROM golang:1.27-alpine AS build

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY *.go ./
RUN CGO_ENABLED=0 GOOS=linux go build -trimpath -ldflags="-s -w" -o /notification

FROM gcr.io/distroless/static-debian12:nonroot

COPY --from=build /notification /notification

EXPOSE 80

ENTRYPOINT [ "/notification" ]
