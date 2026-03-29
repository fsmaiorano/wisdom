#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# migrate.sh — EF Core migration helper for Wisdom.Course
#
# Usage:
#   ./migrate.sh add <MigrationName>   Create a new migration
#   ./migrate.sh update [<Target>]     Apply migrations (optional target name)
#   ./migrate.sh list                  List all migrations
#   ./migrate.sh remove                Remove the last unapplied migration
#
# Run from anywhere — the script always resolves paths relative to its location.
# dotnet ef is invoked from inside Wisdom.Course.Api so BuildHost dirs stay there.
# ---------------------------------------------------------------------------

set -euo pipefail

# Absolute path to the solution root (directory containing this script)
SOLUTION_DIR="$(cd "$(dirname "$0")" && pwd)"

# dotnet ef paths (relative to SOLUTION_DIR/Wisdom.Course.Api)
PROJECT="../Wisdom.Course.Infrastructure.Migration"
NAMESPACE="Wisdom.Course.Infrastructure.Migrations"
MIGRATIONS_DIR="Migrations"

COMMAND="${1:-}"

# Run a dotnet ef command from inside the startup project directory.
# This ensures BuildHost dirs are created inside Wisdom.Course.Api/bin, not at the solution root.
run_ef() {
  (cd "$SOLUTION_DIR/Wisdom.Course.Api" && dotnet ef "$@" --project "$PROJECT")
}

case "$COMMAND" in
  add)
    NAME="${2:-}"
    if [[ -z "$NAME" ]]; then
      echo "Error: migration name is required."
      echo "Usage: ./migrate.sh add <MigrationName>"
      exit 1
    fi
    echo "Creating migration '$NAME'..."
    run_ef migrations add "$NAME" --output-dir "$MIGRATIONS_DIR"
    ;;

  update)
    TARGET="${2:-}"
    if [[ -n "$TARGET" ]]; then
      echo "Updating database to migration '$TARGET'..."
      run_ef database update "$TARGET"
    else
      echo "Applying all pending migrations..."
      run_ef database update
    fi
    ;;

  list)
    echo "Listing migrations..."
    run_ef migrations list
    ;;

  remove)
    echo "Removing last migration..."
    run_ef migrations remove
    ;;

  *)
    echo "Wisdom.Course — EF Core migration helper"
    echo ""
    echo "Usage:"
    echo "  ./migrate.sh add <MigrationName>   Create a new migration"
    echo "  ./migrate.sh update [<Target>]     Apply migrations (optional target)"
    echo "  ./migrate.sh list                  List all migrations"
    echo "  ./migrate.sh remove                Remove last unapplied migration"
    exit 1
    ;;
esac
