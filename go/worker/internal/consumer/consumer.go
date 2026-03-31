package consumer

import (
	"encoding/json"
	"fmt"
	"log"

	"wisdom-worker/internal/models"

	amqp "github.com/rabbitmq/amqp091-go"
)

const (
	exchange  = "wisdom.MediaReadyIntegrationEvent"
	queueName = "wisdom.MediaReadyIntegrationEvent.GoWorker"
	dlxName   = "wisdom.MediaReadyIntegrationEvent.GoWorker.dlx"
	dlqName   = "wisdom.MediaReadyIntegrationEvent.GoWorker.dlq"
	dlqTTLMs  = int32(604_800_000) // 7 days
	prefetch  = 10
)

type Handler func(event models.MediaReadyIntegrationEvent)

func StartConsumer(url string, handler Handler) error {
	conn, err := amqp.Dial(url)
	if err != nil {
		return fmt.Errorf("dial: %w", err)
	}
	defer conn.Close()

	ch, err := setupChannel(conn)
	if err != nil {
		return err
	}
	defer ch.Close()

	if err := ch.Qos(prefetch, 0, false); err != nil {
		return fmt.Errorf("qos: %w", err)
	}

	msgs, err := ch.Consume(queueName, "", false, false, false, false, nil)
	if err != nil {
		return fmt.Errorf("consume: %w", err)
	}

	connClose := conn.NotifyClose(make(chan *amqp.Error, 1))
	chClose := ch.NotifyClose(make(chan *amqp.Error, 1))

	log.Println("Waiting for messages on", queueName)

	for {
		select {
		case msg, ok := <-msgs:
			if !ok {
				return nil // channel closed by broker — caller reconnects
			}
			processMessage(msg, handler)
		case err := <-chClose:
			if err != nil {
				return fmt.Errorf("channel closed: %w", err)
			}
			return nil
		case err := <-connClose:
			if err != nil {
				return fmt.Errorf("connection closed: %w", err)
			}
			return nil
		}
	}
}

// setupChannel opens a channel and declares the full topology:
//
//	Exchange : wisdom.MediaReadyIntegrationEvent              (fanout, durable)
//	DLX      : wisdom.MediaReadyIntegrationEvent.GoWorker.dlx (fanout, durable)
//	DLQ      : wisdom.MediaReadyIntegrationEvent.GoWorker.dlq (durable, 7-day TTL)
//	Queue    : wisdom.MediaReadyIntegrationEvent.GoWorker     (durable, x-dead-letter-exchange → DLX)
//
// On 406 PRECONDITION_FAILED (queue already exists without DLX arg),
// the stale queue is deleted and recreated with the correct topology.
func setupChannel(conn *amqp.Connection) (*amqp.Channel, error) {
	ch, err := conn.Channel()
	if err != nil {
		return nil, fmt.Errorf("open channel: %w", err)
	}

	if err := declareTopology(ch); err == nil {
		return ch, nil
	} else if amqpErr, ok := err.(*amqp.Error); !ok || amqpErr.Code != amqp.PreconditionFailed {
		ch.Close()
		return nil, err
	}

	// 406: queue exists with incompatible arguments (no DLX) — recreate on a fresh channel.
	log.Printf("Queue %q has incompatible arguments — recreating with DLX topology", queueName)

	fresh, err := conn.Channel()
	if err != nil {
		return nil, fmt.Errorf("open fresh channel: %w", err)
	}

	if _, err := fresh.QueueDelete(queueName, false, false, false); err != nil {
		fresh.Close()
		return nil, fmt.Errorf("delete stale queue: %w", err)
	}

	if err := declareTopology(fresh); err != nil {
		fresh.Close()
		return nil, err
	}

	log.Printf("Queue %q recreated with DLX topology", queueName)
	return fresh, nil
}

func declareTopology(ch *amqp.Channel) error {
	// Main exchange (fanout, durable) — also declared by the .NET publisher; idempotent.
	if err := ch.ExchangeDeclare(exchange, "fanout", true, false, false, false, nil); err != nil {
		return fmt.Errorf("declare exchange: %w", err)
	}

	// Dead-letter exchange.
	if err := ch.ExchangeDeclare(dlxName, "fanout", true, false, false, false, nil); err != nil {
		return fmt.Errorf("declare dlx: %w", err)
	}

	// Dead-letter queue — rejected or expired messages land here (7-day TTL).
	if _, err := ch.QueueDeclare(dlqName, true, false, false, false, amqp.Table{
		"x-message-ttl": dlqTTLMs,
	}); err != nil {
		return fmt.Errorf("declare dlq: %w", err)
	}
	if err := ch.QueueBind(dlqName, "", dlxName, false, nil); err != nil {
		return fmt.Errorf("bind dlq: %w", err)
	}

	// Main queue — durable; rejected messages route to DLX → DLQ.
	if _, err := ch.QueueDeclare(queueName, true, false, false, false, amqp.Table{
		"x-dead-letter-exchange": dlxName,
	}); err != nil {
		return fmt.Errorf("declare queue: %w", err)
	}
	if err := ch.QueueBind(queueName, "", exchange, false, nil); err != nil {
		return fmt.Errorf("bind queue: %w", err)
	}

	return nil
}

func processMessage(msg amqp.Delivery, handler Handler) {
	var event models.MediaReadyIntegrationEvent
	if err := json.Unmarshal(msg.Body, &event); err != nil {
		log.Printf("Error parsing event: %v — rejecting to DLQ", err)
		_ = msg.Nack(false, false) // requeue=false → routed to DLX → DLQ
		return
	}
	handler(event)
	_ = msg.Ack(false)
}
