#!/bin/bash
set -euo pipefail

VAULT_ADDR="https://localhost:8200"
VAULT_KEYS_FILE=".vault-keys"
ROLE_NAME="auth-service"
POLICY_NAME="auth-service-policy"

export VAULT_SKIP_VERIFY=true
export VAULT_ADDR=$VAULT_ADDR

if [ ! -f "$VAULT_KEYS_FILE" ]; then
  echo "$VAULT_KEYS_FILE не найден, сначала выполните init-vault.sh"
  exit 1
fi

VAULT_TOKEN=$(grep 'Initial Root Token:' $VAULT_KEYS_FILE | awk '{print $NF}')
export VAULT_TOKEN

echo "🔐 Logging in to Vault..."
vault login $VAULT_TOKEN

echo "🔧 Enabling AppRole auth method..."
vault auth enable approle || true

echo "📜 Creating policy '$POLICY_NAME'..."
vault policy write $POLICY_NAME ./config/auth-service-policy.hcl

echo "👤 Creating AppRole '$ROLE_NAME'..."
vault write auth/approle/role/$ROLE_NAME \
  token_policies="$POLICY_NAME" \
  token_ttl="1h" \
  token_max_ttl="4h"

echo "🔑 Getting RoleID and SecretID..."
ROLE_ID=$(vault read -field=role_id auth/approle/role/$ROLE_NAME/role-id)
SECRET_ID=$(vault write -f -field=secret_id auth/approle/role/$ROLE_NAME/secret-id)

echo "Trying enable kv secret engine"
vault secrets enable -path=secret/ kv

echo "📝 Writing AuthConnection secret..."
vault kv put secret/dev/auth-service AuthConnection="Server=localhost:1433;Database=AuthDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;"

echo ""
echo "✅ Done."
echo "ROLE_ID=$ROLE_ID"
echo "SECRET_ID=$SECRET_ID"
