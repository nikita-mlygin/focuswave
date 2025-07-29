path "secret/data/dev/auth-service" {
  capabilities = ["read", "list"]
}

path "secret/data/dev/auth-service/*" {
  capabilities = ["read", "list"]
}

path "secret/metadata/dev/auth-service" {
  capabilities = ["list"]
}

path "secret/metadata/dev/auth-service/*" {
  capabilities = ["list"]
}

path "secret/dev/auth-service" {
  capabilities = ["read", "list"]
}

path "secret/dev/auth-service/*" {
  capabilities = ["read", "list"]
}