package main

import (
	"errors"
	"log"
	"net/http"

	dapr "github.com/dapr/go-sdk/client"
	"github.com/dapr/go-sdk/service/common"
	daprd "github.com/dapr/go-sdk/service/http"
)

var notificationSubscription = &common.Subscription{
	PubsubName: "messagebus",
	Topic:      "notification-received",
	Route:      "/notification",
}

func main() {
	// One client for the whole process. Creating and closing a client per
	// event closed the shared gRPC connection and crashed the next event.
	client, err := dapr.NewClient()
	if err != nil {
		log.Fatalf("error creating dapr client: %v", err)
	}
	defer client.Close()

	handler := NewNotificationHandler(client)

	s := daprd.NewService(":80")

	if err := s.AddTopicEventHandler(notificationSubscription, handler.Handle); err != nil {
		log.Fatalf("error adding topic subscription: %v", err)
	}

	if err := s.Start(); err != nil && !errors.Is(err, http.ErrServerClosed) {
		log.Fatalf("error listening: %v", err)
	}
}
