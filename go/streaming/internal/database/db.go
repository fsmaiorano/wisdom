package database

import (
	"fmt"

	"github.com/jackc/pgx"
)

type DB struct {
	Pool *pgx.ConnPool
}

func New(connStr string) (*DB, error) {
	cfg, err := pgx.ParseConnectionString(connStr)
	if err != nil {
		return nil, fmt.Errorf("parse connection string: %w", err)
	}
	pool, err := pgx.NewConnPool(pgx.ConnPoolConfig{
		ConnConfig:     cfg,
		MaxConnections: 5,
	})
	if err != nil {
		return nil, fmt.Errorf("create pool: %w", err)
	}
	return &DB{Pool: pool}, nil
}

// MediaStatus returns the status of a media record, or an error if not found.
func (db *DB) MediaStatus(mediaID string) (string, error) {
	var status string
	err := db.Pool.QueryRow(
		`SELECT "Status" FROM "Media" WHERE "Id" = $1::uuid`,
		mediaID,
	).Scan(&status)
	if err != nil {
		return "", fmt.Errorf("query media %s: %w", mediaID, err)
	}
	return status, nil
}
