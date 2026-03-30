package main

import (
	"log"

	"wisdom-worker/internal/config"
	"wisdom-worker/internal/consumer"
	"wisdom-worker/internal/models"
)

func main() {
	cfg := config.Load()

	log.Fatal(consumer.StartConsumer(cfg.RabbitURL, handleMediaReady))
}
func handleMediaReady(event models.MediaReadyIntegrationEvent) {
	log.Printf("Received MediaReadyIntegrationEvent: MediaId=%s LessonId=%s MediaType=%s FilePath=%s",
		event.MediaId, event.LessonId, event.MediaType, event.FilePath)

	// TODO: process the media (e.g. transcode, update DB, notify other modules)
}
