# Быстрое тестирование Awesome Files API

## 1. Запуск API сервера

```bash
cd src/AwesomeFiles.Api
dotnet run
```

API будет доступен по адресу: https://localhost:7000

## 2. Тестирование через curl

### Список файлов
```bash
curl -k https://localhost:7000/api/files | jq
```

### Создание архива
```bash
curl -k -X POST https://localhost:7000/api/archives \
  -H "Content-Type: application/json" \
  -d '{"files": ["test1.txt", "test2.txt", "subfolder/nested.txt"]}' | jq
```

### Проверка статуса (замените {TASK_ID})
```bash
TASK_ID="ваш-task-id-здесь"
curl -k https://localhost:7000/api/archives/$TASK_ID/status | jq
```

### Скачивание архива
```bash
curl -k -O https://localhost:7000/api/archives/$TASK_ID/download
```

## 3. Тестирование через консольный клиент

### Сборка клиента
```bash
cd src/AwesomeFiles.Client
dotnet build
```

### Команды клиента
```bash
# Список файлов
dotnet run list

# Создание архива
dotnet run create-archive test1.txt test2.txt

# Автоматическое создание и скачивание
dotnet run auto-create-and-download test1.txt test2.txt my-archive.zip
```

## 4. Тестирование ошибок

### Несуществующий файл
```bash
curl -k -X POST https://localhost:7000/api/archives \
  -H "Content-Type: application/json" \
  -d '{"files": ["nonexistent.txt"]}' | jq
```

### Path traversal атака
```bash
curl -k -X POST https://localhost:7000/api/archives \
  -H "Content-Type: application/json" \
  -d '{"files": ["../secret.txt"]}' | jq
```

## 5. Swagger UI

Откройте в браузере: https://localhost:7000

## 6. Docker тестирование

```bash
cd docker
docker-compose up --build
```

API будет доступен по адресу: http://localhost:8080

## Ожидаемые результаты

### Успешное создание архива:
```json
{
  "id": "12345678-1234-1234-1234-123456789abc"
}
```

### Статус обработки:
```json
{
  "id": "12345678-1234-1234-1234-123456789abc",
  "status": "Processing",
  "progress": 50,
  "message": "Processing file 2/3"
}
```

### Готовый архив:
```json
{
  "id": "12345678-1234-1234-1234-123456789abc",
  "status": "Ready",
  "progress": 100,
  "message": "Archive ready"
}
```

### Ошибка валидации:
```json
{
  "error": "Files not found",
  "files": ["nonexistent.txt"]
}
```
