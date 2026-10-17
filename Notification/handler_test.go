package main

import (
	"context"
	"errors"
	"testing"

	dapr "github.com/dapr/go-sdk/client"
	"github.com/dapr/go-sdk/service/common"
)

type fakeBinding struct {
	calls []*dapr.InvokeBindingRequest
	err   error
}

func (f *fakeBinding) InvokeOutputBinding(_ context.Context, in *dapr.InvokeBindingRequest) error {
	f.calls = append(f.calls, in)
	return f.err
}

func TestHandle_ValidMessage_InvokesBinding(t *testing.T) {
	fake := &fakeBinding{}
	h := NewNotificationHandler(fake)

	retry, err := h.Handle(context.Background(), &common.TopicEvent{RawData: []byte(`{"message":"hello"}`)})

	if err != nil || retry {
		t.Fatalf("want (false, nil), got (%v, %v)", retry, err)
	}
	if len(fake.calls) != 1 {
		t.Fatalf("want 1 call, got %d", len(fake.calls))
	}
	call := fake.calls[0]
	if call.Name != "notification-storage" || call.Operation != "create" {
		t.Errorf("unexpected binding %q/%q", call.Name, call.Operation)
	}
	if string(call.Data) != `"hello"` {
		t.Errorf("want data %q, got %q", `"hello"`, call.Data)
	}
}

func TestHandle_PropertyNameIsCaseInsensitive(t *testing.T) {
	fake := &fakeBinding{}
	h := NewNotificationHandler(fake)

	_, err := h.Handle(context.Background(), &common.TopicEvent{RawData: []byte(`{"Message":"hi"}`)})

	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if string(fake.calls[0].Data) != `"hi"` {
		t.Errorf("got %q", fake.calls[0].Data)
	}
}

func TestHandle_EscapesMessage(t *testing.T) {
	fake := &fakeBinding{}
	h := NewNotificationHandler(fake)

	_, _ = h.Handle(context.Background(), &common.TopicEvent{RawData: []byte(`{"message":"a \"b\"\n"}`)})

	if string(fake.calls[0].Data) != `"a \"b\"\n"` {
		t.Errorf("got %q", fake.calls[0].Data)
	}
}

func TestHandle_MultipleEvents_ReuseBinding(t *testing.T) {
	fake := &fakeBinding{}
	h := NewNotificationHandler(fake)

	for i := 0; i < 5; i++ {
		if retry, err := h.Handle(context.Background(), &common.TopicEvent{RawData: []byte(`{"message":"m"}`)}); err != nil || retry {
			t.Fatalf("event %d: got (%v, %v)", i, retry, err)
		}
	}
	if len(fake.calls) != 5 {
		t.Errorf("want 5 calls, got %d", len(fake.calls))
	}
}

func TestHandle_InvalidJSON_DropsWithoutBinding(t *testing.T) {
	fake := &fakeBinding{}
	h := NewNotificationHandler(fake)

	retry, err := h.Handle(context.Background(), &common.TopicEvent{RawData: []byte(`not json`)})

	if err == nil || retry {
		t.Fatalf("want (false, error), got (%v, %v)", retry, err)
	}
	if len(fake.calls) != 0 {
		t.Errorf("binding must not be called")
	}
}

func TestHandle_BindingError_Retries(t *testing.T) {
	fake := &fakeBinding{err: errors.New("boom")}
	h := NewNotificationHandler(fake)

	retry, err := h.Handle(context.Background(), &common.TopicEvent{RawData: []byte(`{"message":"x"}`)})

	if err == nil || !retry {
		t.Fatalf("want (true, error), got (%v, %v)", retry, err)
	}
}
