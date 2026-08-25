$SCRIPT_DIR = $PSScriptRoot

docker compose -f "$SCRIPT_DIR\..\docker-compose.yml" down -v --remove-orphans
Remove-Item -Path "$SCRIPT_DIR\..\terraform\terraform.tfstate", "$SCRIPT_DIR\..\terraform\terraform.tfstate.backup", "$SCRIPT_DIR\..\terraform\tfplan" -ErrorAction SilentlyContinue
Write-Host "Tudo limpo!"
