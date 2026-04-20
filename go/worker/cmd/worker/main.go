package main

import (
	"log"
	"time"

	"wisdom-worker/internal/config"
	"wisdom-worker/internal/consumer"
	"wisdom-worker/internal/database"
	"wisdom-worker/internal/processor"
	"wisdom-worker/internal/storage"
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
	defer db.Pool.Close()
	log.Println("Connected to database")

	store, err := storage.NewMinioClient(cfg.MinioURL, cfg.MinioUsername, cfg.MinioPassword)
	if err != nil {
		log.Fatalf("Failed to create MinIO client: %v", err)
	}
	log.Println("MinIO client ready")

	mediaProcessor := processor.New(db, store)

	backoff := initialBackoff
	for {
		log.Println("Connecting to RabbitMQ...")
		err := consumer.StartConsumer(cfg.RabbitURL, mediaProcessor.Handle)
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
