VAULT_ADDR="https://localhost:8200"
VAULT_KEYS_FILE=".vault-keys"
ROLE_NAME="auth-service"
POLICY_NAME="auth-service-policy"

VAULT_TOKEN=$(grep 'Initial Root Token:' $VAULT_KEYS_FILE | awk '{print $NF}')
export VAULT_TOKEN

echo "🔐 Logging in to Vault..."

vault login $VAULT_TOKEN