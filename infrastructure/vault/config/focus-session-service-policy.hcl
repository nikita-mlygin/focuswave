path "secret/data/dev/focus-session-service" {
  capabilities = ["read", "list"]
}

path "secret/data/dev/focus-session-service/*" {
  capabilities = ["read", "list"]
}

path "secret/metadata/dev/focus-session-service" {
  capabilities = ["list"]
}

path "secret/metadata/dev/focus-session-service/*" {
  capabilities = ["list"]
}

path "secret/dev/focus-session-service" {
  capabilities = ["read", "list"]
}

path "secret/dev/focus-session-service/*" {
  capabilities = ["read", "list"]
}