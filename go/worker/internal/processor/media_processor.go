package processor

import (
	"context"
	"fmt"
	"io"
	"log"
	"os"
	"path/filepath"
	"time"

	"wisdom-worker/internal/database"
	"wisdom-worker/internal/models"
	"wisdom-worker/internal/storage"
	"wisdom-worker/internal/video"
)

// MediaProcessor orchestrates the full video processing pipeline for a
// MediaReadyIntegrationEvent: download → transcode → upload → mark ready.
type MediaProcessor struct {
	db    *database.DB
	store *storage.MinioClient
}

func New(db *database.DB, store *storage.MinioClient) *MediaProcessor {
	return &MediaProcessor{db: db, store: store}
}

// Handle is the consumer.Handler implementation. It is called once per
// incoming event and manages the entire lifecycle: status transitions,
// ffmpeg processing and MinIO uploads.
func (p *MediaProcessor) Handle(event models.MediaReadyIntegrationEvent) {
	log.Printf("Received MediaReadyIntegrationEvent: MediaId=%s LessonId=%s MediaType=%s FilePath=%s",
		event.MediaId, event.LessonId, event.MediaType, event.FilePath)

	ctx := context.Background()

	if err := p.setStatus(event.MediaId, "Processing"); err != nil {
		log.Printf("media %s: failed to set Processing status: %v", event.MediaId, err)
		return
	}

	if err := p.process(ctx, event); err != nil {
		log.Printf("media %s: processing failed: %v", event.MediaId, err)
		_ = p.setStatus(event.MediaId, "Failed")
		return
	}
}

func (p *MediaProcessor) process(ctx context.Context, event models.MediaReadyIntegrationEvent) error {
	tmpPath, err := p.downloadToTemp(ctx, event.FilePath)
	if err != nil {
		return fmt.Errorf("download: %w", err)
	}
	defer os.Remove(tmpPath)

	output, err := video.Process(ctx, tmpPath)
	if err != nil {
		return fmt.Errorf("transcode: %w", err)
	}
	defer os.RemoveAll(output.TempDir)

	thumbnailKey, err := p.uploadResults(ctx, event.MediaId, output)
	if err != nil {
		return err
	}

	if err := p.markReady(event.MediaId, thumbnailKey, output.Duration); err != nil {
		return fmt.Errorf("mark ready: %w", err)
	}

	log.Printf("media %s is Ready (duration=%ds, thumbnail=%s)", event.MediaId, output.Duration, thumbnailKey)
	return nil
}

func (p *MediaProcessor) uploadResults(ctx context.Context, mediaId string, output *video.ProcessedOutput) (string, error) {
	prefix := "media/" + mediaId
	thumbnailKey := prefix + "/thumbnail.jpg"

	if err := p.store.Upload(ctx, thumbnailKey, output.ThumbnailLocalPath, "image/jpeg"); err != nil {
		return "", fmt.Errorf("upload thumbnail: %w", err)
	}

	for label, localPath := range output.Videos {
		key := prefix + "/" + label + ".mp4"
		if err := p.store.Upload(ctx, key, localPath, "video/mp4"); err != nil {
			return "", fmt.Errorf("upload %s: %w", label, err)
		}
		log.Printf("media %s: uploaded %s", mediaId, key)
	}

	return thumbnailKey, nil
}

func (p *MediaProcessor) markReady(mediaId, thumbnailKey string, duration int) error {
	_, err := p.db.Pool.Exec(
		`UPDATE "Media" SET "Status" = $1, "ThumbnailPath" = $2, "Duration" = $3, "LastModifiedAt" = $4 WHERE "Id" = $5::uuid`,
		"Ready",
		thumbnailKey,
		duration,
		time.Now().UTC(),
		mediaId,
	)
	return err
}

func (p *MediaProcessor) setStatus(mediaId, status string) error {
	_, err := p.db.Pool.Exec(
		`UPDATE "Media" SET "Status" = $1, "LastModifiedAt" = $2 WHERE "Id" = $3::uuid`,
		status,
		time.Now().UTC(),
		mediaId,
	)
	return err
}

func (p *MediaProcessor) downloadToTemp(ctx context.Context, objectPath string) (string, error) {
	reader, err := p.store.Download(ctx, objectPath)
	if err != nil {
		return "", err
	}
	defer reader.Close()

	ext := filepath.Ext(objectPath)
	tmp, err := os.CreateTemp("", "wisdom-raw-*"+ext)
	if err != nil {
		return "", err
	}
	defer tmp.Close()

	if _, err := io.Copy(tmp, reader); err != nil {
		os.Remove(tmp.Name())
		return "", err
	}
	return tmp.Name(), nil
}
