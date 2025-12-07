package main

import (
	"context"
	"errors"
	"testing"

	dapr "github.com/dapr/go-sdk/client"
	"github.com/dapr/go-sdk/service/common"
)

func TestNotificationEventHandler_Success(t *testing.T) {
	// Save original invoker
	originalInvoker := currentInvoker
	defer func() { currentInvoker = originalInvoker }()

	// Create mock invoker
	currentInvoker = func(ctx context.Context, in *dapr.InvokeBindingRequest) error {
		// Verify the binding request
		if in.Name != "notification-storage" {
			t.Errorf("Expected binding name 'notification-storage', got '%s'", in.Name)
		}
		if in.Operation != "create" {
			t.Errorf("Expected operation 'create', got '%s'", in.Operation)
		}
		return nil
	}

	// Create test event
	testData := []byte(`{"message": "test notification"}`)
	event := &common.TopicEvent{
		RawData: testData,
	}

	// Call the handler
	retry, err := notificationEventHandler(context.Background(), event)

	// Verify results
	if err != nil {
		t.Errorf("Expected no error, got %v", err)
	}
	if retry {
		t.Error("Expected retry to be false")
	}
}

func TestNotificationEventHandler_Error(t *testing.T) {
	// Save original invoker
	originalInvoker := currentInvoker
	defer func() { currentInvoker = originalInvoker }()

	// Create mock invoker that returns an error
	expectedError := errors.New("binding invocation failed")
	currentInvoker = func(ctx context.Context, in *dapr.InvokeBindingRequest) error {
		return expectedError
	}

	// Create test event
	testData := []byte(`{"message": "test notification"}`)
	event := &common.TopicEvent{
		RawData: testData,
	}

	// Call the handler
	retry, err := notificationEventHandler(context.Background(), event)

	// Verify results
	if err == nil {
		t.Error("Expected an error, got nil")
	}
	if err != expectedError {
		t.Errorf("Expected error '%v', got '%v'", expectedError, err)
	}
	if !retry {
		t.Error("Expected retry to be true on error")
	}
}

func TestNotificationEventHandler_VerifyBindingParameters(t *testing.T) {
	// Save original invoker
	originalInvoker := currentInvoker
	defer func() { currentInvoker = originalInvoker }()

	testData := []byte(`{"message": "test notification", "id": 123}`)
	var capturedRequest *dapr.InvokeBindingRequest

	// Create mock invoker that captures the request
	currentInvoker = func(ctx context.Context, in *dapr.InvokeBindingRequest) error {
		capturedRequest = in
		return nil
	}

	// Create test event
	event := &common.TopicEvent{
		RawData: testData,
		Topic:   "notification-received",
	}

	// Call the handler
	retry, err := notificationEventHandler(context.Background(), event)

	// Verify results
	if err != nil {
		t.Errorf("Expected no error, got %v", err)
	}
	if retry {
		t.Error("Expected retry to be false")
	}

	// Verify the captured request
	if capturedRequest == nil {
		t.Fatal("Expected request to be captured, got nil")
	}
	if capturedRequest.Name != "notification-storage" {
		t.Errorf("Expected binding name 'notification-storage', got '%s'", capturedRequest.Name)
	}
	if capturedRequest.Operation != "create" {
		t.Errorf("Expected operation 'create', got '%s'", capturedRequest.Operation)
	}
	if string(capturedRequest.Data) != string(testData) {
		t.Errorf("Expected data '%s', got '%s'", string(testData), string(capturedRequest.Data))
	}
}

func TestNotificationEventHandler_WithContext(t *testing.T) {
	// Save original invoker
	originalInvoker := currentInvoker
	defer func() { currentInvoker = originalInvoker }()

	contextChecked := false

	// Create mock invoker that checks context
	currentInvoker = func(ctx context.Context, in *dapr.InvokeBindingRequest) error {
		// Verify context is passed through
		if ctx == nil {
			t.Error("Context should not be nil")
		}
		contextChecked = true
		return nil
	}

	// Create test event
	event := &common.TopicEvent{
		RawData: []byte(`{"test": "data"}`),
	}

	// Call the handler with a context
	ctx := context.Background()
	_, _ = notificationEventHandler(ctx, event)

	if !contextChecked {
		t.Error("Context was not checked in mock invoker")
	}
}

func TestNotificationEventHandler_EmptyData(t *testing.T) {
	// Save original invoker
	originalInvoker := currentInvoker
	defer func() { currentInvoker = originalInvoker }()

	// Create mock invoker
	currentInvoker = func(ctx context.Context, in *dapr.InvokeBindingRequest) error {
		if len(in.Data) != 0 {
			t.Errorf("Expected empty data, got %d bytes", len(in.Data))
		}
		return nil
	}

	// Create test event with empty data
	event := &common.TopicEvent{
		RawData: []byte{},
	}

	// Call the handler
	retry, err := notificationEventHandler(context.Background(), event)

	// Verify results
	if err != nil {
		t.Errorf("Expected no error, got %v", err)
	}
	if retry {
		t.Error("Expected retry to be false")
	}
}

func TestNotificationEventHandler_MultipleErrors(t *testing.T) {
	// Save original invoker
	originalInvoker := currentInvoker
	defer func() { currentInvoker = originalInvoker }()

	testCases := []struct {
		name          string
		error         error
		expectedRetry bool
	}{
		{
			name:          "Network error",
			error:         errors.New("network timeout"),
			expectedRetry: true,
		},
		{
			name:          "Storage error",
			error:         errors.New("storage unavailable"),
			expectedRetry: true,
		},
		{
			name:          "Permission error",
			error:         errors.New("access denied"),
			expectedRetry: true,
		},
	}

	for _, tc := range testCases {
		t.Run(tc.name, func(t *testing.T) {
			// Create mock invoker that returns the test error
			currentInvoker = func(ctx context.Context, in *dapr.InvokeBindingRequest) error {
				return tc.error
			}

			// Create test event
			event := &common.TopicEvent{
				RawData: []byte(`{"test": "data"}`),
			}

			// Call the handler
			retry, err := notificationEventHandler(context.Background(), event)

			// Verify results
			if err == nil {
				t.Error("Expected an error, got nil")
			}
			if err != tc.error {
				t.Errorf("Expected error '%v', got '%v'", tc.error, err)
			}
			if retry != tc.expectedRetry {
				t.Errorf("Expected retry to be %v, got %v", tc.expectedRetry, retry)
			}
		})
	}
}
