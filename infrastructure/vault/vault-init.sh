#!/bin/bash
set -euo pipefail

VAULT_ADDR="https://localhost:8200"
VAULT_KEYS_FILE=".vault-keys"

export VAULT_SKIP_VERIFY=true
export VAULT_ADDR=$VAULT_ADDR

echo "Ожидаем запуска Vault..."
# until vault status &> /dev/null; do
#   sleep 1
# done

# if ! vault status | grep -q 'Initialized.*true'; then
echo "Vault не инициализирован. Инициализация..."
vault operator init -key-shares=5 -key-threshold=3 > $VAULT_KEYS_FILE
echo "Vault инициализирован. Ключи сохранены в $VAULT_KEYS_FILE"
# fi

# if vault status | grep -q 'Sealed.*true'; then

# fi
