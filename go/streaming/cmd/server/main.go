package main

import (
	"context"
	"errors"
	"log"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"wisdom-streaming/internal/config"
	"wisdom-streaming/internal/database"
	"wisdom-streaming/internal/handler"
	"wisdom-streaming/internal/storage"
)

func main() {
	cfg := config.Load()

	db, err := database.New(cfg.DBConnectionString)
	if err != nil {
		log.Fatalf("db connect: %v", err)
	}
	defer db.Pool.Close()
	log.Println("Connected to database")

	store, err := storage.New(cfg.MinioURL, cfg.MinioAccessKey, cfg.MinioSecretKey)
	if err != nil {
		log.Fatalf("minio client: %v", err)
	}
	log.Println("MinIO client ready")

	streamHandler := handler.New(db, store, cfg.InternalAPIKey, cfg.DisableAuth)

	mux := http.NewServeMux()
	mux.Handle("/stream/", streamHandler)
	mux.HandleFunc("/health", func(w http.ResponseWriter, _ *http.Request) {
		w.WriteHeader(http.StatusOK)
	})

	srv := &http.Server{
		Addr:              cfg.Addr,
		Handler:           mux,
		ReadHeaderTimeout: 10 * time.Second,
	}

	ctx, cancel := signal.NotifyContext(context.Background(), os.Interrupt, syscall.SIGTERM)
	defer cancel()

	go func() {
		log.Printf("Streaming server listening on %s", cfg.Addr)
		if err := srv.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
			log.Fatalf("listen: %v", err)
		}
	}()

	<-ctx.Done()
	log.Println("Shutting down...")
	shutdownCtx, shutdownCancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer shutdownCancel()
	if err := srv.Shutdown(shutdownCtx); err != nil {
		log.Printf("graceful shutdown: %v", err)
	}
	log.Println("Server stopped")
}
