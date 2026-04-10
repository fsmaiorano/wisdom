package database

import (
	"github.com/jackc/pgx"
)

type DB struct {
	Conn *pgx.Conn
}

func NewDB(connectionString string) (*DB, error) {
	cfg, err := pgx.ParseConnectionString(connectionString)
	if err != nil {
		return nil, err
	}
	conn, err := pgx.Connect(cfg)
	if err != nil {
		return nil, err
	}
	return &DB{Conn: conn}, nil
}
