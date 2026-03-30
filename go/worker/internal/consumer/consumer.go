package consumer

import (
	"encoding/json"
	"log"

	"wisdom-worker/internal/models"

	"github.com/rabbitmq/amqp091-go"
)

const (
	exchange  = "wisdom.MediaReadyIntegrationEvent"
	queueName = "wisdom.MediaReadyIntegrationEvent.GoWorker"
)

type Handler func(event models.MediaReadyIntegrationEvent)

func StartConsumer(url string, handler Handler) error {
	conn, err := amqp091.Dial(url)
	if err != nil {
		return err
	}

	ch, err := conn.Channel()
	if err != nil {
		return err
	}

	err = ch.ExchangeDeclare(
		exchange,
		"fanout",
		true,
		false,
		false,
		false,
		nil,
	)
	if err != nil {
		return err
	}

	q, err := ch.QueueDeclare(
		queueName,
		true,
		false,
		false,
		false,
		nil,
	)
	if err != nil {
		return err
	}

	err = ch.QueueBind(q.Name, "", exchange, false, nil)
	if err != nil {
		return err
	}

	err = ch.Qos(10, 0, false)
	if err != nil {
		return err
	}

	msgs, err := ch.Consume(
		q.Name,
		"",
		false,
		false,
		false,
		false,
		nil,
	)
	if err != nil {
		return err
	}

	log.Println("Waiting for messages on", queueName)

	for msg := range msgs {
		var event models.MediaReadyIntegrationEvent

		if err := json.Unmarshal(msg.Body, &event); err != nil {
			log.Println("Error parsing event:", err)
			_ = msg.Nack(false, false)
			continue
		}

		handler(event)
		_ = msg.Ack(false)
	}

	return nil
}
