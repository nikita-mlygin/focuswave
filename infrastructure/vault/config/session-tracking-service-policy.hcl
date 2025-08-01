path "secret/data/dev/session-tracking-service" {
  capabilities = ["read", "list"]
}

path "secret/data/dev/session-tracking-service/*" {
  capabilities = ["read", "list"]
}

path "secret/metadata/dev/session-tracking-service" {
  capabilities = ["list"]
}

path "secret/metadata/dev/session-tracking-service/*" {
  capabilities = ["list"]
}

path "secret/dev/session-tracking-service" {
  capabilities = ["read", "list"]
}

path "secret/dev/session-tracking-service/*" {
  capabilities = ["read", "list"]
}