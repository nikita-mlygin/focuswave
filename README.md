# focuswave

focuswave — трекер фокуса и перерывов с использованием микросервисов на .NET.

## архитектура

- auth-service — сервис аутентификации и авторизации (IdentityServer + ASP.NET Core Identity)  
- focus-session-service — управление фокус-сессиями  
- frontend — UI на React  

## запуск локально

1. клонировать репозиторий  
2. собрать и запустить сервисы через docker-compose  
3. открыть UI в браузере

## требования

- .NET 8 или новее  
- Docker и Docker Compose  
- Kubernetes (например, k3d или kind)

## структура проекта

```plaintext
src/  
├── auth-service/  
├── focus-session-service/  
├── frontend/  
docker-compose.yml  
README.md  
focuswave.sln  
```