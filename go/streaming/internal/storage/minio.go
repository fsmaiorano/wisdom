package storage

import (
	"context"
	"fmt"
	"io"

	"github.com/minio/minio-go/v7"
	"github.com/minio/minio-go/v7/pkg/credentials"
)

const bucket = "wisdom-media"

type Client struct {
	mc *minio.Client
}

func New(endpoint, accessKey, secretKey string) (*Client, error) {
	mc, err := minio.New(endpoint, &minio.Options{
		Creds:  credentials.NewStaticV4(accessKey, secretKey, ""),
		Secure: false,
	})
	if err != nil {
		return nil, fmt.Errorf("minio client: %w", err)
	}
	return &Client{mc: mc}, nil
}

// ObjectInfo holds the metadata returned by StatObject.
type ObjectInfo struct {
	Size        int64
	ContentType string
	ETag        string
}

// Stat returns metadata for the object at objectPath.
func (c *Client) Stat(ctx context.Context, objectPath string) (ObjectInfo, error) {
	info, err := c.mc.StatObject(ctx, bucket, objectPath, minio.StatObjectOptions{})
	if err != nil {
		return ObjectInfo{}, fmt.Errorf("stat %s: %w", objectPath, err)
	}
	return ObjectInfo{
		Size:        info.Size,
		ContentType: info.ContentType,
		ETag:        info.ETag,
	}, nil
}

// GetRange fetches bytes [start, end] (both inclusive) from objectPath.
// Pass end = 0 together with start = 0 and use Get instead for full objects.
func (c *Client) GetRange(ctx context.Context, objectPath string, start, end int64) (io.ReadCloser, error) {
	opts := minio.GetObjectOptions{}
	if err := opts.SetRange(start, end); err != nil {
		return nil, fmt.Errorf("set range [%d-%d]: %w", start, end, err)
	}
	obj, err := c.mc.GetObject(ctx, bucket, objectPath, opts)
	if err != nil {
		return nil, fmt.Errorf("get range %s [%d-%d]: %w", objectPath, start, end, err)
	}
	return obj, nil
}

// Get fetches the full object at objectPath.
func (c *Client) Get(ctx context.Context, objectPath string) (io.ReadCloser, error) {
	obj, err := c.mc.GetObject(ctx, bucket, objectPath, minio.GetObjectOptions{})
	if err != nil {
		return nil, fmt.Errorf("get %s: %w", objectPath, err)
	}
	return obj, nil
}
