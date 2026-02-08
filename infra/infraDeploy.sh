#!/bin/bash
set -e

# ============ CONFIG ============
RG="tfg-infra-rg"
LOCATION="westeurope"

APP_NAME="tfgapi"
ENV_NAME="tfg-env"

DOCKER_USER="vprietosainf"
IMAGE_NAME="$DOCKER_USER/tfgapi:latest"

DOCKERFILE_NAME="Dockerfile"

# bash infraDeploy.shIMPORTANTE: estás ejecutando desde /infra
API_DIR="../src/TfgApi"

echo "==> 1) Creando Resource Group: $RG"
az group create --name "$RG" --location "$LOCATION" >/dev/null

echo "==> 2) Creando/Verificando Container Apps Environment: $ENV_NAME"
az containerapp env show --name "$ENV_NAME" --resource-group "$RG" >/dev/null 2>&1 || \
az containerapp env create \
  --name "$ENV_NAME" \
  --resource-group "$RG" \
  --location "$LOCATION" >/dev/null

echo "==> 3) Build & Push Docker image: $IMAGE_NAME"
cd "$API_DIR"

docker build -t "$IMAGE_NAME" -f "$DOCKERFILE_NAME" .
docker push "$IMAGE_NAME"

cd - >/dev/null

echo "==> 4) Deploy Azure Container App: $APP_NAME"
az containerapp up \
  --name "$APP_NAME" \
  --resource-group "$RG" \
  --environment "$ENV_NAME" \
  --image "$IMAGE_NAME" \
  --ingress external \
  --target-port 8080

echo "✅ Listo. URL:"
az containerapp show -n "$APP_NAME" -g "$RG" --query properties.configuration.ingress.fqdn -o tsv
