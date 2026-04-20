package reconciler

import (
	"context"
	"log"
	"time"

	"github.com/google/uuid"

	"wisdom-worker/internal/consumer"
	"wisdom-worker/internal/database"
	"wisdom-worker/internal/models"
)

// Reconciler periodically scans for Media rows stuck in "Uploaded" or "Failed"
// status and re-dispatches them through the normal processing pipeline.
//
// It atomically claims eligible rows (sets Status = 'Processing') before
// dispatching, so concurrent runs never process the same entry twice.
type Reconciler struct {
	db         *database.DB
	handler    consumer.Handler
	interval   time.Duration
	staleAfter time.Duration
}

// New creates a Reconciler.
//   - interval:   how often the reconciler polls (e.g. 1 minute)
//   - staleAfter: how long a row must be in Uploaded/Failed before being retried (e.g. 5 minutes)
func New(db *database.DB, handler consumer.Handler, interval, staleAfter time.Duration) *Reconciler {
	return &Reconciler{
		db:         db,
		handler:    handler,
		interval:   interval,
		staleAfter: staleAfter,
	}
}

func (r *Reconciler) Start(ctx context.Context) {
	log.Printf("Reconciler started (interval=%s, staleAfter=%s)", r.interval, r.staleAfter)

	r.runOnce(ctx)

	ticker := time.NewTicker(r.interval)
	defer ticker.Stop()

	for {
		select {
		case <-ticker.C:
			r.runOnce(ctx)
		case <-ctx.Done():
			log.Println("Reconciler stopped")
			return
		}
	}
}

func (r *Reconciler) runOnce(ctx context.Context) {
	cutoff := time.Now().UTC().Add(-r.staleAfter)

	const claimSQL = `
		UPDATE "Media"
		SET    "Status" = 'Processing',
		       "LastModifiedAt" = NOW()
		WHERE  "Status" IN ('Uploaded', 'Failed')
		  AND  "LastModifiedAt" < $1
		RETURNING "Id", "LessonId", "Type", "FilePath"`

	rows, err := r.db.Pool.QueryEx(ctx, claimSQL, nil, cutoff)
	if err != nil {
		log.Printf("Reconciler: query failed: %v", err)
		return
	}
	defer rows.Close()

	var claimed []models.MediaReadyIntegrationEvent
	for rows.Next() {
		var id, lessonID, mediaType, filePath string
		if err := rows.Scan(&id, &lessonID, &mediaType, &filePath); err != nil {
			log.Printf("Reconciler: scan failed: %v", err)
			continue
		}
		claimed = append(claimed, models.MediaReadyIntegrationEvent{
			EventId:    uuid.NewString(),
			OccurredAt: time.Now().UTC(),
			MediaId:    id,
			LessonId:   lessonID,
			MediaType:  mediaType,
			FilePath:   filePath,
		})
	}
	if err := rows.Err(); err != nil {
		log.Printf("Reconciler: rows error: %v", err)
	}

	if len(claimed) == 0 {
		return
	}

	log.Printf("Reconciler: dispatching %d stale media entries for reprocessing", len(claimed))
	for _, event := range claimed {
		log.Printf("Reconciler: retrying media %s (filePath=%s)", event.MediaId, event.FilePath)
		r.handler(event)
	}
}
