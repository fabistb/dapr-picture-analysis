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

// Global Dapr client to be reused across requests.
// The Dapr client is thread-safe and designed to be shared across goroutines.
// This prevents resource exhaustion from creating clients per request.
var daprClient dapr.Client

// clientInvoker is a function type that allows dependency injection for testing
type clientInvoker func(ctx context.Context, in *dapr.InvokeBindingRequest) error

// currentInvoker is the function used to invoke bindings, can be overridden for testing
var currentInvoker clientInvoker

func main() {
	// Initialize Dapr client once at startup
	var err error
	daprClient, err = dapr.NewClient()
	if err != nil {
		log.Fatalf("error creating Dapr client: %v", err)
	}
	defer daprClient.Close()

	// Set the default invoker to use the real client
	currentInvoker = func(ctx context.Context, in *dapr.InvokeBindingRequest) error {
		return daprClient.InvokeOutputBinding(ctx, in)
	}

	s := daprd.NewService(":8080")

	if err := s.AddTopicEventHandler(notificationSubscription, notificationEventHandler); err != nil {
		log.Fatalf("error adding topic subscription: %v", err)
	}

	if err := s.Start(); err != nil && err != http.ErrServerClosed {
		log.Fatalf("error listening: %v", err)
	}
}

func notificationEventHandler(ctx context.Context, e *common.TopicEvent) (retry bool, err error) {
	in := &dapr.InvokeBindingRequest{
		Name:      "notification-storage",
		Operation: "create",
		Data:      e.RawData,
	}

	if err := currentInvoker(ctx, in); err != nil {
		log.Printf("error invoking output binding: %v", err)
		return true, err
	}

	log.Printf("notification processed successfully")
	return false, nil
}
