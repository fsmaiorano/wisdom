#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# sqlscript.sh — EF Core SQL script generator for Wisdom.Media
#
# Usage:
#   ./sqlscript.sh complete              One idempotent SQL file for all migrations
#   ./sqlscript.sh per-migration         One SQL file per migration step
#   ./sqlscript.sh complete --no-idempotent
#   ./sqlscript.sh per-migration --no-idempotent
#
# Run from anywhere — the script always resolves paths relative to its location.
# dotnet ef is invoked from inside Wisdom.Media.Api so BuildHost dirs stay there.
# Output: Wisdom.Media.Infrastructure.Migration/Scripts/
# ---------------------------------------------------------------------------

set -euo pipefail

# Absolute path to the solution root (directory containing this script)
SOLUTION_DIR="$(cd "$(dirname "$0")" && pwd)"

# dotnet ef paths (relative to SOLUTION_DIR/Wisdom.Media.Api)
PROJECT="../Wisdom.Media.Infrastructure.Migration"

# Output goes here (absolute path so it works regardless of CWD)
OUT_DIR="$SOLUTION_DIR/Wisdom.Media.Infrastructure.Migration/Scripts"

# ---------- helpers ---------------------------------------------------------

usage() {
  echo "Wisdom.Media — EF Core SQL script generator"
  echo ""
  echo "Usage:"
  echo "  ./sqlscript.sh complete              One idempotent script for all migrations"
  echo "  ./sqlscript.sh per-migration         One script per migration step"
  echo ""
  echo "Options:"
  echo "  --no-idempotent    Omit idempotency guards (default: idempotent)"
  echo ""
  echo "Output: Wisdom.Media.Infrastructure.Migration/Scripts/"
  exit 1
}

# Run a dotnet ef command from inside the startup project directory.
# This ensures BuildHost dirs are created inside Wisdom.Media.Api/bin, not at the solution root.
run_ef() {
  (cd "$SOLUTION_DIR/Wisdom.Media.Api" && dotnet ef "$@" --project "$PROJECT")
}

# Collect migration names in Media (strips status suffix and build noise)
get_migrations() {
  run_ef migrations list --no-build 2>/dev/null \
  | grep -E '^[0-9]{14}_' \
  | awk '{print $1}'
}

# ---------- argument parsing ------------------------------------------------

COMMAND="${1:-}"
IDEMPOTENT="--idempotent"

for arg in "$@"; do
  [[ "$arg" == "--no-idempotent" ]] && IDEMPOTENT=""
done

case "$COMMAND" in
  complete|per-migration) ;;
  *) usage ;;
esac

# ---------- pre-flight ------------------------------------------------------

echo "Building solution..."
(cd "$SOLUTION_DIR/Wisdom.Media.Api" && dotnet build --nologo -v quiet)

mkdir -p "$OUT_DIR"

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# ---------- complete --------------------------------------------------------

if [[ "$COMMAND" == "complete" ]]; then
  OUTPUT="$OUT_DIR/${TIMESTAMP}_complete.sql"
  echo "Generating complete script → Wisdom.Media.Infrastructure.Migration/Scripts/$(basename "$OUTPUT")"

  run_ef migrations script \
    --output "$OUTPUT" \
    --no-build \
    $IDEMPOTENT

  echo "Done."
fi

# ---------- per-migration ---------------------------------------------------

if [[ "$COMMAND" == "per-migration" ]]; then
  MIGRATIONS=()
  while IFS= read -r line; do
    [[ -n "$line" ]] && MIGRATIONS+=("$line")
  done <<< "$(get_migrations)"

  if [[ ${#MIGRATIONS[@]} -eq 0 ]]; then
    echo "No migrations found."
    exit 1
  fi

  STEP_DIR="$OUT_DIR/${TIMESTAMP}_per_migration"
  mkdir -p "$STEP_DIR"

  echo "Found ${#MIGRATIONS[@]} migration(s). Generating scripts in Wisdom.Media.Infrastructure.Migration/Scripts/$(basename "$STEP_DIR")/"
  echo ""

  PREV="0"
  IDX=1

  for MIGRATION in "${MIGRATIONS[@]}"; do
    PADDED=$(printf "%03d" "$IDX")
    OUTPUT="$STEP_DIR/${PADDED}_${MIGRATION}.sql"

    echo "  [$PADDED] $PREV → $MIGRATION"

    run_ef migrations script "$PREV" "$MIGRATION" \
      --output "$OUTPUT" \
      --no-build \
      $IDEMPOTENT

    PREV="$MIGRATION"
    ((IDX++))
  done

  echo ""
  echo "Done."
  ls "$STEP_DIR/"
fi
