#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# ── Cores para output ──
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

log()  { echo -e "${GREEN}[✓]${NC} $1"; }
warn() { echo -e "${YELLOW}[!]${NC} $1"; }
info() { echo -e "${CYAN}[→]${NC} $1"; }

CLEAN=0
if [ "${1:-}" == "--clean" ]; then
    CLEAN=1
    info "Modo --clean ativado. Parando e destruindo containers existentes..."
    docker compose -f "$SCRIPT_DIR/../docker-compose.yml" down --remove-orphans 2>/dev/null || true
else
    info "Verificando infraestrutura existente (use --clean para recriar do zero)..."
fi

# ── Rebuild e start ──
info "Construindo e subindo infraestrutura..."
PUBSUB_EMULATOR_HOST=pubsub:8085 \
STORAGE_EMULATOR_HOST=http://gcs:4443/storage/v1/ \
VisualApiUrl=http://mvfc-image-vision-api:5000 \
docker compose -f "$SCRIPT_DIR/../docker-compose.yml" up -d --build

# ── Variáveis de ambiente para Terraform e Validação (Host local) ──
export PUBSUB_EMULATOR_HOST=localhost:8085
export GOOGLE_PUBSUB_CUSTOM_ENDPOINT=http://$PUBSUB_EMULATOR_HOST/v1/
export GOOGLE_STORAGE_CUSTOM_ENDPOINT=http://localhost:4443/storage/v1/
export GOOGLE_CLOUD_PROJECT=local-project
VISION_LOCAL_URL=http://localhost:5000

# ── Aguardar emuladores ──
info "Aguardando PubSub emulator..."
until curl -sf "http://$PUBSUB_EMULATOR_HOST" >/dev/null 2>&1; do sleep 1; done
log "PubSub pronto"

info "Aguardando GCS emulator..."
until curl -sf "${GOOGLE_STORAGE_CUSTOM_ENDPOINT}b" >/dev/null 2>&1; do sleep 1; done
log "GCS pronto"

info "Aguardando Vision API..."
until curl -sf "${VISION_LOCAL_URL}/health" >/dev/null 2>&1; do sleep 1; done
log "Vision API pronta"

# ── Terraform ──
info "Aplicando Terraform..."
cd "$SCRIPT_DIR/../terraform"

if [ "$CLEAN" == "1" ]; then
    rm -f terraform.tfstate terraform.tfstate.backup tfplan
fi

terraform init -upgrade -input=false
terraform fmt
terraform validate
terraform apply -auto-approve -input=false
cd "$SCRIPT_DIR"

log "Infraestrutura pronta!"
echo ""
info "Endpoints disponíveis:"
echo "  Dashboard:   http://localhost:3000"
echo "  Upload API:  http://localhost:8081/upload"
echo "  Vision API:  http://localhost:5000/health"
echo "  GCS Buckets: http://localhost:4443/storage/v1/b"
echo "  PubSub:      http://localhost:8085"