$ErrorActionPreference = "Stop"

$SCRIPT_DIR = $PSScriptRoot

# Cores para output
function Write-Log($msg) { Write-Host "[✓] $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "[!] $msg" -ForegroundColor Yellow }
function Write-Info($msg) { Write-Host "[→] $msg" -ForegroundColor Cyan }

$CLEAN = $false
if ($args.Count -gt 0 -and $args[0] -eq "--clean") {
    $CLEAN = $true
    Write-Info "Modo --clean ativado. Parando e destruindo containers existentes..."
    docker compose -f "$SCRIPT_DIR\..\docker-compose.yml" down --remove-orphans 2>$null
} else {
    Write-Info "Verificando infraestrutura existente (use --clean para recriar do zero)..."
}

# Rebuild e start
Write-Info "Construindo e subindo infraestrutura..."
$env:PUBSUB_EMULATOR_HOST = "pubsub:8681"
$env:STORAGE_EMULATOR_HOST = "http://gcs:4443/storage/v1/"
$env:VisualApiUrl = "http://mvfc-image-vision-api:5000"

docker compose -f "$SCRIPT_DIR\..\docker-compose.yml" up -d --build

# Variáveis de ambiente para Terraform e Validação (Host local)
$env:PUBSUB_EMULATOR_HOST = "localhost:8681"
$env:GOOGLE_PUBSUB_CUSTOM_ENDPOINT = "http://$($env:PUBSUB_EMULATOR_HOST)/v1/"
$env:GOOGLE_STORAGE_CUSTOM_ENDPOINT = "http://localhost:4443/storage/v1/"
$env:GOOGLE_CLOUD_PROJECT = "local-project"
$env:VISION_LOCAL_URL = "http://localhost:5000"

# Aguardar emuladores
Write-Info "Aguardando PubSub emulator..."
while ($true) {
    try {
        $response = Invoke-WebRequest -Uri "http://$($env:PUBSUB_EMULATOR_HOST)" -UseBasicParsing -ErrorAction SilentlyContinue
        if ($null -ne $response) { break }
    } catch {
        Start-Sleep -Seconds 1
    }
}
Write-Log "PubSub pronto"

Write-Info "Aguardando GCS emulator..."
while ($true) {
    try {
        $response = Invoke-WebRequest -Uri "$($env:GOOGLE_STORAGE_CUSTOM_ENDPOINT)b" -UseBasicParsing -ErrorAction SilentlyContinue
        if ($null -ne $response) { break }
    } catch {
        Start-Sleep -Seconds 1
    }
}
Write-Log "GCS pronto"

Write-Info "Aguardando Vision API..."
while ($true) {
    try {
        $response = Invoke-WebRequest -Uri "$($env:VISION_LOCAL_URL)/health" -UseBasicParsing -ErrorAction SilentlyContinue
        if ($null -ne $response) { break }
    } catch {
        Start-Sleep -Seconds 1
    }
}
Write-Log "Vision API pronta"

# Terraform
Write-Info "Aplicando Terraform..."
Push-Location "$SCRIPT_DIR\..\terraform"

if ($CLEAN) {
    Remove-Item -Path "terraform.tfstate", "terraform.tfstate.backup", "tfplan" -ErrorAction SilentlyContinue
}

terraform init -upgrade -input=false
terraform fmt
terraform validate
terraform apply -auto-approve -input=false
Pop-Location

Write-Log "Infraestrutura pronta!"
Write-Host ""
Write-Info "Endpoints disponíveis:"
Write-Host "  Dashboard:   http://localhost:3000"
Write-Host "  Upload API:  http://localhost:8081/upload"
Write-Host "  Vision API:  http://localhost:5000/health"
Write-Host "  GCS Buckets: http://localhost:4443/storage/v1/b"
Write-Host "  PubSub:      http://localhost:8681"
