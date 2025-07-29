VAULT_ADDR="https://localhost:8200"
VAULT_KEYS_FILE=".vault-keys"

export VAULT_SKIP_VERIFY=true
export VAULT_ADDR=$VAULT_ADDR

echo "Unsealing Vault..."
for key in $(grep 'Unseal Key' $VAULT_KEYS_FILE | awk '{print $4}' | head -n 3); do
    vault operator unseal $key
    echo 'Unsealing...'
done
echo "Vault unsealed ✅"