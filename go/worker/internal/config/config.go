package config

import (
	"log"
	"os"
	"path/filepath"
	"strconv"
	"time"

	"github.com/joho/godotenv"
)

type Config struct {
	RabbitURL            string
	S3Endpoint           string
	S3AccessKey          string
	S3SecretKey          string
	DBConnectionString   string
	ReconcilerInterval   time.Duration
	ReconcilerStaleAfter time.Duration
}

func loadEnv() {
	dir, _ := os.Getwd()
	for {
		candidate := filepath.Join(dir, ".env")
		if _, err := os.Stat(candidate); err == nil {
			if err := godotenv.Load(candidate); err != nil {
				log.Printf("Warning: found %s but could not load it: %v", candidate, err)
			} else {
				log.Printf("Loaded env from %s", candidate)
			}
			return
		}
		parent := filepath.Dir(dir)
		if parent == dir {
			log.Println("No .env file found, relying on environment variables")
			return
		}
		dir = parent
	}
}

func Load() *Config {
	loadEnv()
	return &Config{
		RabbitURL:            os.Getenv("RABBITMQ_URL"),
		S3Endpoint:           os.Getenv("S3_ENDPOINT"),
		S3AccessKey:          os.Getenv("S3_ACCESS_KEY"),
		S3SecretKey:          os.Getenv("S3_SECRET_KEY"),
		DBConnectionString:   os.Getenv("DB_CONN"),
		ReconcilerInterval:   envDuration("RECONCILER_INTERVAL_SECONDS", 60),
		ReconcilerStaleAfter: envDuration("RECONCILER_STALE_SECONDS", 300),
	}
}

func envDuration(key string, defaultSecs int) time.Duration {
	if v := os.Getenv(key); v != "" {
		if n, err := strconv.Atoi(v); err == nil && n > 0 {
			return time.Duration(n) * time.Second
		}
		log.Printf("Warning: invalid value for %s, using default %ds", key, defaultSecs)
	}
	return time.Duration(defaultSecs) * time.Second
}
