package storage

import (
	"context"
	"fmt"
	"io"

	"github.com/minio/minio-go/v7"
	"github.com/minio/minio-go/v7/pkg/credentials"
)

const BucketName = "wisdom-media"

type S3Client struct {
	client *minio.Client
}

func NewS3Client(endpoint, accessKey, secretKey string) (*S3Client, error) {
	client, err := minio.New(endpoint, &minio.Options{
		Creds:  credentials.NewStaticV4(accessKey, secretKey, ""),
		Secure: false,
	})
	if err != nil {
		return nil, fmt.Errorf("create S3 client: %w", err)
	}
	return &S3Client{client: client}, nil
}

func (m *S3Client) Download(ctx context.Context, objectPath string) (io.ReadCloser, error) {
	obj, err := m.client.GetObject(ctx, BucketName, objectPath, minio.GetObjectOptions{})
	if err != nil {
		return nil, fmt.Errorf("get object %s: %w", objectPath, err)
	}
	return obj, nil
}

func (m *S3Client) Upload(ctx context.Context, objectPath, filePath, contentType string) error {
	_, err := m.client.FPutObject(ctx, BucketName, objectPath, filePath, minio.PutObjectOptions{
		ContentType: contentType,
	})
	if err != nil {
		return fmt.Errorf("upload %s: %w", objectPath, err)
	}
	return nil
}

func (m *S3Client) Delete(ctx context.Context, objectPath string) error {
	err := m.client.RemoveObject(ctx, BucketName, objectPath, minio.RemoveObjectOptions{})
	if err != nil {
		return fmt.Errorf("delete object %s: %w", objectPath, err)
	}
	return nil
}
