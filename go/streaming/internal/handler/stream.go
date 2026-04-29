package handler

import (
	"fmt"
	"io"
	"log"
	"net/http"
	"path"
	"strconv"
	"strings"

	"wisdom-streaming/internal/database"
	"wisdom-streaming/internal/storage"
)

// StreamHandler serves video files from MinIO with HTTP range request support.
type StreamHandler struct {
	db     *database.DB
	store  *storage.Client
	apiKey string
	noAuth bool
}

func New(db *database.DB, store *storage.Client, apiKey string, noAuth bool) *StreamHandler {
	return &StreamHandler{db: db, store: store, apiKey: apiKey, noAuth: noAuth}
}

func (h *StreamHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet && r.Method != http.MethodHead {
		http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
		return
	}

	if !h.noAuth {
		if r.Header.Get("X-Internal-Api-Key") != h.apiKey {
			http.Error(w, "unauthorized", http.StatusUnauthorized)
			return
		}
	}

	// Path: /stream/{mediaId}
	mediaID := path.Base(r.URL.Path)
	if mediaID == "" || mediaID == "." || mediaID == "/" {
		http.Error(w, "missing media id", http.StatusBadRequest)
		return
	}

	quality := r.URL.Query().Get("quality")
	switch quality {
	case "480p", "720p", "1080p":
	case "":
		quality = "720p"
	default:
		http.Error(w, "invalid quality: use 480p, 720p, or 1080p", http.StatusBadRequest)
		return
	}

	status, err := h.db.MediaStatus(mediaID)
	if err != nil {
		log.Printf("stream: media %s not found: %v", mediaID, err)
		http.Error(w, "media not found", http.StatusNotFound)
		return
	}
	if status != "Ready" {
		http.Error(w, fmt.Sprintf("media not ready (status=%s)", status), http.StatusNotFound)
		return
	}

	objectPath := fmt.Sprintf("media/%s/%s.mp4", mediaID, quality)
	ctx := r.Context()

	info, err := h.store.Stat(ctx, objectPath)
	if err != nil {
		log.Printf("stream: stat %s: %v", objectPath, err)
		http.Error(w, "video file not found", http.StatusNotFound)
		return
	}

	w.Header().Set("Content-Type", "video/mp4")
	w.Header().Set("Accept-Ranges", "bytes")
	w.Header().Set("Cache-Control", "no-cache")

	if r.Method == http.MethodHead {
		w.Header().Set("Content-Length", strconv.FormatInt(info.Size, 10))
		w.WriteHeader(http.StatusOK)
		return
	}

	rangeHeader := r.Header.Get("Range")
	if rangeHeader == "" {
		h.serveFullContent(w, r, objectPath, info.Size)
		return
	}

	start, end, ok := parseRange(rangeHeader, info.Size)
	if !ok {
		w.Header().Set("Content-Range", fmt.Sprintf("bytes */%d", info.Size))
		http.Error(w, "range not satisfiable", http.StatusRequestedRangeNotSatisfiable)
		return
	}

	h.serveRange(w, r, objectPath, start, end, info.Size)
}

func (h *StreamHandler) serveFullContent(w http.ResponseWriter, r *http.Request, objectPath string, size int64) {
	rc, err := h.store.Get(r.Context(), objectPath)
	if err != nil {
		log.Printf("stream: get %s: %v", objectPath, err)
		http.Error(w, "failed to read video", http.StatusInternalServerError)
		return
	}
	defer rc.Close()

	w.Header().Set("Content-Length", strconv.FormatInt(size, 10))
	w.WriteHeader(http.StatusOK)
	io.Copy(w, rc)
}

func (h *StreamHandler) serveRange(w http.ResponseWriter, r *http.Request, objectPath string, start, end, total int64) {
	rc, err := h.store.GetRange(r.Context(), objectPath, start, end)
	if err != nil {
		log.Printf("stream: get range %s [%d-%d]: %v", objectPath, start, end, err)
		http.Error(w, "failed to read video", http.StatusInternalServerError)
		return
	}
	defer rc.Close()

	chunkSize := end - start + 1
	w.Header().Set("Content-Range", fmt.Sprintf("bytes %d-%d/%d", start, end, total))
	w.Header().Set("Content-Length", strconv.FormatInt(chunkSize, 10))
	w.WriteHeader(http.StatusPartialContent)
	io.Copy(w, rc)
}

// parseRange parses a "bytes=start-end" Range header.
// Returns (start, end, ok) with end inclusive. Handles suffix ranges (bytes=-N).
func parseRange(header string, size int64) (start, end int64, ok bool) {
	if !strings.HasPrefix(header, "bytes=") {
		return 0, 0, false
	}
	spec := strings.TrimPrefix(header, "bytes=")

	// Only handle a single range spec (no comma-separated multi-range).
	if strings.Contains(spec, ",") {
		return 0, 0, false
	}

	parts := strings.SplitN(spec, "-", 2)
	if len(parts) != 2 {
		return 0, 0, false
	}

	if parts[0] == "" {
		// Suffix range: bytes=-N → last N bytes
		n, err := strconv.ParseInt(parts[1], 10, 64)
		if err != nil || n <= 0 {
			return 0, 0, false
		}
		start = size - n
		if start < 0 {
			start = 0
		}
		end = size - 1
	} else {
		s, err := strconv.ParseInt(parts[0], 10, 64)
		if err != nil || s < 0 {
			return 0, 0, false
		}
		start = s

		if parts[1] == "" {
			// bytes=N- → from N to end
			end = size - 1
		} else {
			e, err := strconv.ParseInt(parts[1], 10, 64)
			if err != nil || e < start {
				return 0, 0, false
			}
			end = e
		}
	}

	if start >= size || end >= size || start > end {
		return 0, 0, false
	}
	return start, end, true
}
