package main

import (
	"context"
	"log"
	"os"
	"os/signal"
	"syscall"
	"time"

	"wisdom-worker/internal/config"
	"wisdom-worker/internal/consumer"
	"wisdom-worker/internal/database"
	"wisdom-worker/internal/processor"
	"wisdom-worker/internal/reconciler"
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

	store, err := storage.NewS3Client(cfg.S3Endpoint, cfg.S3AccessKey, cfg.S3SecretKey)
	if err != nil {
		log.Fatalf("Failed to create S3 client: %v", err)
	}
	log.Println("S3 client ready")

	mediaProcessor := processor.New(db, store)

	// Context for background workers; cancelled on SIGINT/SIGTERM.
	ctx, cancel := signal.NotifyContext(context.Background(), os.Interrupt, syscall.SIGTERM)
	defer cancel()

	// Start the reconciler in the background.
	rec := reconciler.New(db, mediaProcessor.Handle, cfg.ReconcilerInterval, cfg.ReconcilerStaleAfter)
	go rec.Start(ctx)

	// Consumer reconnect loop — runs until the process is interrupted.
	backoff := initialBackoff
	for {
		select {
		case <-ctx.Done():
			log.Println("Shutting down consumer")
			return
		default:
		}

		log.Println("Connecting to RabbitMQ...")
		err := consumer.StartConsumer(cfg.RabbitURL, mediaProcessor.Handle)
		if err != nil {
			log.Printf("Consumer error: %v — reconnecting in %s", err, backoff)
			select {
			case <-time.After(backoff):
			case <-ctx.Done():
				log.Println("Shutting down consumer")
				return
			}
			if backoff < maxBackoff {
				backoff *= 2
			}
		} else {
			log.Println("Consumer disconnected — reconnecting")
			backoff = initialBackoff
		}
	}
}
