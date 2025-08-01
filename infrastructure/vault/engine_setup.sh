. ./sh-env.sh

vault secrets enable -path=secret -version=2 kv

echo "🔧 Enabling KV v2 and AppRole..."

vault auth enable approle || true

./provision-auth-service.sh
./provision-focus-session-service.sh
./provision-session-tracking-service.sh

echo "✅ Vault provisioning complete. See ./secrets/*.env for credentials."
