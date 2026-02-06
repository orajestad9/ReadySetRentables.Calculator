#!/bin/bash
set -euo pipefail

DEPLOY_DIR="$HOME/apps/ReadySetRentables.Deploy"   # <-- your "compose host" folder
API_DIR="$DEPLOY_DIR/api"                          # <-- clone of API repo

echo "Deploying API only..."

# 1) Update API repo
cd "$API_DIR"
git fetch origin
git reset --hard origin/main

# 2) Build + restart ONLY the API container
cd "$DEPLOY_DIR"
docker compose up -d --build rsr-api

# 3) Optional: show status
docker compose ps rsr-api
echo "API deploy complete."