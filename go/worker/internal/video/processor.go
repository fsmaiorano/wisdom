package video

import (
	"context"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"strconv"
	"strings"
)

type Resolution struct {
	Height string
	Label  string
}

var Resolutions = []Resolution{
	{"480", "480p"},
	{"720", "720p"},
	{"1080", "1080p"},
}

type ProcessedOutput struct {
	ThumbnailLocalPath string
	Videos             map[string]string // label -> local temp path
	Duration           int               // seconds
	TempDir            string
}

// Process takes a local video file and produces a thumbnail and transcoded
// versions at 480p, 720p and 1080p. The caller is responsible for cleaning
// up TempDir (e.g. defer os.RemoveAll(output.TempDir)).
func Process(ctx context.Context, inputPath string) (*ProcessedOutput, error) {
	dir, err := os.MkdirTemp("", "wisdom-video-*")
	if err != nil {
		return nil, fmt.Errorf("create temp dir: %w", err)
	}

	duration, err := getDuration(ctx, inputPath)
	if err != nil {
		os.RemoveAll(dir)
		return nil, fmt.Errorf("get duration: %w", err)
	}

	thumbnailPath := filepath.Join(dir, "thumbnail.jpg")
	if err := takeScreenshot(ctx, inputPath, thumbnailPath, thumbnailTimestamp(duration)); err != nil {
		os.RemoveAll(dir)
		return nil, fmt.Errorf("take screenshot: %w", err)
	}

	videos := make(map[string]string, len(Resolutions))
	for _, r := range Resolutions {
		out := filepath.Join(dir, r.Label+".mp4")
		if err := transcode(ctx, inputPath, out, r.Height); err != nil {
			os.RemoveAll(dir)
			return nil, fmt.Errorf("transcode %s: %w", r.Label, err)
		}
		videos[r.Label] = out
	}

	return &ProcessedOutput{
		ThumbnailLocalPath: thumbnailPath,
		Videos:             videos,
		Duration:           duration,
		TempDir:            dir,
	}, nil
}

func getDuration(ctx context.Context, inputPath string) (int, error) {
	cmd := exec.CommandContext(ctx, "ffprobe",
		"-v", "error",
		"-show_entries", "format=duration",
		"-of", "default=noprint_wrappers=1:nokey=1",
		inputPath,
	)
	out, err := cmd.Output()
	if err != nil {
		return 0, fmt.Errorf("ffprobe: %w", err)
	}
	d, err := strconv.ParseFloat(strings.TrimSpace(string(out)), 64)
	if err != nil {
		return 0, fmt.Errorf("parse duration: %w", err)
	}
	return int(d), nil
}

func thumbnailTimestamp(durationSecs int) string {
	ts := 5
	if durationSecs < 10 {
		ts = 1
	}
	return fmt.Sprintf("00:00:%02d", ts)
}

func takeScreenshot(ctx context.Context, inputPath, outputPath, timestamp string) error {
	cmd := exec.CommandContext(ctx, "ffmpeg",
		"-y",
		"-ss", timestamp,
		"-i", inputPath,
		"-frames:v", "1",
		"-q:v", "2",
		outputPath,
	)
	if out, err := cmd.CombinedOutput(); err != nil {
		return fmt.Errorf("%w\n%s", err, out)
	}
	return nil
}

func transcode(ctx context.Context, inputPath, outputPath, height string) error {
	cmd := exec.CommandContext(ctx, "ffmpeg",
		"-y",
		"-i", inputPath,
		"-vf", fmt.Sprintf("scale=-2:%s", height),
		"-c:v", "libx264",
		"-crf", "23",
		"-preset", "fast",
		"-c:a", "aac",
		"-b:a", "128k",
		outputPath,
	)
	if out, err := cmd.CombinedOutput(); err != nil {
		return fmt.Errorf("%w\n%s", err, out)
	}
	return nil
}
