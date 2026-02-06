#!/bin/bash
set -e

APP_DIR="$HOME/apps/ReadySetRentables"

echo "Deploying ReadySetRentables..."
cd "$APP_DIR"

echo "Fetching latest code..."
git fetch origin
git reset --hard origin/main

echo "Stopping containers..."
docker compose down

echo "Building and starting containers..."
docker compose up -d --build

echo "Deploy complete."
