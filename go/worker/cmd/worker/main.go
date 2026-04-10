package main

import (
	"log"
	"time"

	"wisdom-worker/internal/config"
	"wisdom-worker/internal/consumer"
	"wisdom-worker/internal/database"
	"wisdom-worker/internal/models"
)

const (
	initialBackoff = 2 * time.Second
	maxBackoff     = 30 * time.Second
)

func main() {
	cfg := config.Load()

	db, err := database.NewDB(cfg.DBConnectionString)
	if err != nil {
		log.Fatalf("Failed to connect to database: %v", err)
	}
	defer db.Conn.Close()
	log.Println("Connected to database")

	backoff := initialBackoff
	for {
		log.Println("Connecting to RabbitMQ...")
		err := consumer.StartConsumer(cfg.RabbitURL, handleMediaReady(db))
		if err != nil {
			log.Printf("Consumer error: %v — reconnecting in %s", err, backoff)
			time.Sleep(backoff)
			if backoff < maxBackoff {
				backoff *= 2
			}
		} else {
			log.Println("Consumer disconnected — reconnecting")
			backoff = initialBackoff
		}
	}
}

func handleMediaReady(db *database.DB) func(models.MediaReadyIntegrationEvent) {
	return func(event models.MediaReadyIntegrationEvent) {
		log.Printf("Received MediaReadyIntegrationEvent: MediaId=%s LessonId=%s MediaType=%s FilePath=%s",
			event.MediaId, event.LessonId, event.MediaType, event.FilePath)

		media, err := db.Conn.Exec("SELECT * FROM media WHERE id = ?", event.MediaId)
		if err != nil {
			log.Printf("Failed to query media: %v", err)
			return
		}

		log.Printf("Media details: %v", media)
	}
}
