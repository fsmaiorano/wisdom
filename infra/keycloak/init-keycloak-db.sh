#!/bin/sh
# Runs inside the postgres container on first-time volume initialization (initdb.d).
# Creates the Keycloak database if it does not already exist.
set -e

psql -v ON_ERROR_STOP=1 -U postgres -tc \
  "SELECT 1 FROM pg_database WHERE datname='KeycloakDb'" | grep -q 1 || \
  createdb -U postgres "KeycloakDb"

echo "[init] Database 'KeycloakDb' is ready."
