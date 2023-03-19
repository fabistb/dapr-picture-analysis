package main

import (
	"context"
	dapr "github.com/dapr/go-sdk/client"
	"log"
	"net/http"

	"github.com/dapr/go-sdk/service/common"
	daprd "github.com/dapr/go-sdk/service/http"
)

var notificationSubscription = &common.Subscription{
	PubsubName: "messagebus",
	Topic:      "notification-received",
	Route:      "/notification",
}

func main() {
	s := daprd.NewService(":8080")

	if err := s.AddTopicEventHandler(notificationSubscription, notificationEventHandler); err != nil {
		log.Fatalf("error adding topic subscription: %v", err)
	}

	if err := s.Start(); err != nil && err != http.ErrServerClosed {
		log.Fatalf("error listenning: %v", err)
	}
}

func notificationEventHandler(ctx context.Context, e *common.TopicEvent) (retry bool, err error) {
	client, err := dapr.NewClient()
	if err != nil {
		panic(err)
	}
	defer client.Close()

	in := &dapr.InvokeBindingRequest{
		Name:      "notification-storage",
		Operation: "create",
		Data:      e.RawData,
	}

	if err := client.InvokeOutputBinding(ctx, in); err != nil {
		panic(err)
	}

	log.Printf("event - PubsubName: %s, Topic: %s, ID: %s, Data: %s", e.PubsubName, e.Topic, e.ID, e.Data)

	return false, nil
}
