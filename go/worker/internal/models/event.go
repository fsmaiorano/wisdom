package models

import "time"

type MediaReadyIntegrationEvent struct {
	EventId       string    `json:"eventId"`
	OccurredAt    time.Time `json:"occurredAt"`
	CorrelationId *string   `json:"correlationId"`
	MediaId       string    `json:"mediaId"`
	LessonId      string    `json:"lessonId"`
	MediaType     string    `json:"mediaType"`
	FilePath      string    `json:"filePath"`
}
