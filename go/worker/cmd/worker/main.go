package main

import (
	"log"
	"time"

	"wisdom-worker/internal/config"
	"wisdom-worker/internal/consumer"
	"wisdom-worker/internal/models"
)

const (
	initialBackoff = 2 * time.Second
	maxBackoff     = 30 * time.Second
)

func main() {
	cfg := config.Load()

	backoff := initialBackoff
	for {
		log.Println("Connecting to RabbitMQ...")
		err := consumer.StartConsumer(cfg.RabbitURL, handleMediaReady)
		if err != nil {
			log.Printf("Consumer error: %v — reconnecting in %s", err, backoff)
			time.Sleep(backoff)
			if backoff < maxBackoff {
				backoff *= 2
			}
		} else {
			// Clean disconnect (broker closed channel/connection) — reconnect immediately.
			log.Println("Consumer disconnected — reconnecting")
			backoff = initialBackoff
		}
	}
}

func handleMediaReady(event models.MediaReadyIntegrationEvent) {
	log.Printf("Received MediaReadyIntegrationEvent: MediaId=%s LessonId=%s MediaType=%s FilePath=%s",
		event.MediaId, event.LessonId, event.MediaType, event.FilePath)

	// TODO: process the media (e.g. transcode, update DB, notify other modules)
}
