#!/bin/bash
set -euo pipefail

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_SERVICE_NAME="readysetrentables-api"

echo "Deploying API only..."

# 1) Update repository in place (single-folder model)
cd "$REPO_DIR"
git fetch origin
git reset --hard origin/main

# 2) Build + restart ONLY the API container
docker compose up -d --build "$API_SERVICE_NAME"

# 3) Optional: show status
docker compose ps "$API_SERVICE_NAME"
echo "API deploy complete."
