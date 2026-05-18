#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

docker compose -f ${SCRIPT_DIR}/../docker-compose.yml down -v --remove-orphans
rm -f ${SCRIPT_DIR}/../terraform/terraform.tfstate ${SCRIPT_DIR}/../terraform/terraform.tfstate.backup ${SCRIPT_DIR}/../terraform/tfplan
echo "Tudo limpo!"
