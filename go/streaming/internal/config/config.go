package config

import (
	"log"
	"os"
	"path/filepath"

	"github.com/joho/godotenv"
)

type Config struct {
	Addr               string
	S3Endpoint         string
	S3AccessKey        string
	S3SecretKey        string
	DBConnectionString string
	InternalAPIKey     string
	DisableAuth        bool
}

func Load() *Config {
	loadEnv()
	return &Config{
		Addr:               envOr("ADDR", ":8080"),
		S3Endpoint:         os.Getenv("S3_ENDPOINT"),
		S3AccessKey:        os.Getenv("S3_ACCESS_KEY"),
		S3SecretKey:        os.Getenv("S3_SECRET_KEY"),
		DBConnectionString: os.Getenv("DB_CONN"),
		InternalAPIKey:     envOr("INTERNAL_API_KEY", "wisdom-internal-2025"),
		DisableAuth:        os.Getenv("DISABLE_AUTH") == "true",
	}
}

func envOr(key, fallback string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return fallback
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
