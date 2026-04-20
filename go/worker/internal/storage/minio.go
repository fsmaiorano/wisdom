package storage

import (
	"context"
	"fmt"
	"io"

	"github.com/minio/minio-go/v7"
	"github.com/minio/minio-go/v7/pkg/credentials"
)

const BucketName = "wisdom-media"

type MinioClient struct {
	client *minio.Client
}

func NewMinioClient(endpoint, accessKey, secretKey string) (*MinioClient, error) {
	client, err := minio.New(endpoint, &minio.Options{
		Creds:  credentials.NewStaticV4(accessKey, secretKey, ""),
		Secure: false,
	})
	if err != nil {
		return nil, fmt.Errorf("create minio client: %w", err)
	}
	return &MinioClient{client: client}, nil
}

func (m *MinioClient) Download(ctx context.Context, objectPath string) (io.ReadCloser, error) {
	obj, err := m.client.GetObject(ctx, BucketName, objectPath, minio.GetObjectOptions{})
	if err != nil {
		return nil, fmt.Errorf("get object %s: %w", objectPath, err)
	}
	return obj, nil
}

func (m *MinioClient) Upload(ctx context.Context, objectPath, filePath, contentType string) error {
	_, err := m.client.FPutObject(ctx, BucketName, objectPath, filePath, minio.PutObjectOptions{
		ContentType: contentType,
	})
	if err != nil {
		return fmt.Errorf("upload %s: %w", objectPath, err)
	}
	return nil
}
