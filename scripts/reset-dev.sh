#!/usr/bin/env bash
set -euo pipefail

skip_up=false
keep_data=false
for arg in "$@"; do
  case "$arg" in
    --skip-up)
      skip_up=true
      ;;
    --keep-data)
      keep_data=true
      ;;
    *)
      echo "Unknown argument: $arg" >&2
      exit 1
      ;;
  esac
done

if [ "$keep_data" = true ]; then
  echo "Stopping dev containers without removing the persisted Postgres volume..."
  docker compose down --remove-orphans
else
  echo "Stopping dev containers and removing the persisted Postgres volume..."
  docker compose down --remove-orphans --volumes
fi

if [ "$skip_up" = true ]; then
  echo "Skipped startup because --skip-up was supplied."
  exit 0
fi

if [ "$keep_data" = true ]; then
  echo "Starting the dev stack while preserving the existing database volume..."
  FEATURES__RESETDATABASEONSTARTUP=false docker compose --profile dev up --build --force-recreate
else
  echo "Starting the dev stack with a fresh database..."
  FEATURES__RESETDATABASEONSTARTUP=true docker compose --profile dev up --build --force-recreate
fi
