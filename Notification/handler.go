package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"

	dapr "github.com/dapr/go-sdk/client"
	"github.com/dapr/go-sdk/service/common"
)

const (
	bindingName      = "notification-storage"
	bindingOperation = "create"
)

// BindingInvoker is the part of the dapr client used by the handler.
type BindingInvoker interface {
	InvokeOutputBinding(ctx context.Context, in *dapr.InvokeBindingRequest) error
}

// NotificationMessage is the payload of a notification-received event.
type NotificationMessage struct {
	Message string `json:"message"`
}

// NotificationHandler stores received notifications via the output binding.
type NotificationHandler struct {
	binding BindingInvoker
}

func NewNotificationHandler(binding BindingInvoker) *NotificationHandler {
	return &NotificationHandler{binding: binding}
}

// Handle processes one event. Invalid payloads are dropped (no retry),
// binding failures are retried.
func (h *NotificationHandler) Handle(ctx context.Context, e *common.TopicEvent) (bool, error) {
	var msg NotificationMessage
	if err := json.Unmarshal(e.RawData, &msg); err != nil {
		return false, fmt.Errorf("invalid notification payload: %w", err)
	}

	// Same as the .NET service: the message text is stored as a JSON string.
	data, err := json.Marshal(msg.Message)
	if err != nil {
		return false, fmt.Errorf("error serializing message: %w", err)
	}

	in := &dapr.InvokeBindingRequest{
		Name:      bindingName,
		Operation: bindingOperation,
		Data:      data,
	}

	if err := h.binding.InvokeOutputBinding(ctx, in); err != nil {
		log.Printf("error invoking binding %q: %v", bindingName, err)
		return true, err
	}

	return false, nil
}
