package database

import (
	"github.com/jackc/pgx"
)

type DB struct {
	Pool *pgx.ConnPool
}

func NewDB(connectionString string) (*DB, error) {
	cfg, err := pgx.ParseConnectionString(connectionString)
	if err != nil {
		return nil, err
	}
	pool, err := pgx.NewConnPool(pgx.ConnPoolConfig{
		ConnConfig:     cfg,
		MaxConnections: 5,
		AcquireTimeout: 0,
	})
	if err != nil {
		return nil, err
	}
	return &DB{Pool: pool}, nil
}
