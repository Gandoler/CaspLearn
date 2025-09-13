# Руководство по тестированию Awesome Files API

## Пошаговая инструкция: create -> poll -> download

### Шаг 1: Подготовка тестовых файлов

```bash
# Создайте папку для тестовых файлов
mkdir -p files
cd files

# Создайте несколько тестовых файлов
echo "Это первый тестовый файл" > test1.txt
echo "Это второй тестовый файл" > test2.txt
echo "Содержимое третьего файла" > test3.txt

# Создайте подпапку с файлом
mkdir subfolder
echo "Файл в подпапке" > subfolder/nested.txt

# Проверьте структуру
tree .
# Результат:
# .
# ├── subfolder
# │   └── nested.txt
# ├── test1.txt
# ├── test2.txt
# └── test3.txt
```

### Шаг 2: Запуск API сервера

```bash
# В первом терминале
cd src/AwesomeFiles.Api
dotnet run

# Ожидаемый вывод:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7000
# info: Microsoft.Hosting.Lifetime[0]
#       Application started. Press Ctrl+C to shut down.
```

### Шаг 3: Тестирование через curl

#### 3.1. Проверка списка файлов

```bash
# В другом терминале
curl -k https://localhost:7000/api/files | jq

# Ожидаемый результат:
# [
#   {
#     "name": "subfolder/nested.txt",
#     "size": 18,
#     "modified": "2025-01-13T12:00:00Z"
#   },
#   {
#     "name": "test1.txt",
#     "size": 25,
#     "modified": "2025-01-13T12:00:00Z"
#   },
#   {
#     "name": "test2.txt",
#     "size": 25,
#     "modified": "2025-01-13T12:00:00Z"
#   },
#   {
#     "name": "test3.txt",
#     "size": 24,
#     "modified": "2025-01-13T12:00:00Z"
#   }
# ]
```

#### 3.2. Создание архива

```bash
curl -k -X POST https://localhost:7000/api/archives \
  -H "Content-Type: application/json" \
  -d '{"files": ["test1.txt", "test2.txt", "subfolder/nested.txt"]}' | jq

# Ожидаемый результат:
# {
#   "id": "12345678-1234-1234-1234-123456789abc"
# }
```

**Сохраните ID из ответа для следующих шагов!**

#### 3.3. Проверка статуса (polling)

```bash
# Замените {TASK_ID} на реальный ID из предыдущего шага
TASK_ID="12345678-1234-1234-1234-123456789abc"

# Проверяем статус
curl -k https://localhost:7000/api/archives/$TASK_ID/status | jq

# Возможные результаты:

# 1. Задача в очереди:
# {
#   "id": "12345678-1234-1234-1234-123456789abc",
#   "status": "Pending",
#   "progress": 0,
#   "message": null
# }

# 2. Архив создается:
# {
#   "id": "12345678-1234-1234-1234-123456789abc",
#   "status": "Processing",
#   "progress": 50,
#   "message": "Processing file 2/3"
# }

# 3. Архив готов:
# {
#   "id": "12345678-1234-1234-1234-123456789abc",
#   "status": "Ready",
#   "progress": 100,
#   "message": "Archive ready"
# }
```

#### 3.4. Автоматический polling (скрипт)

```bash
#!/bin/bash
TASK_ID="12345678-1234-1234-1234-123456789abc"

echo "Ожидание готовности архива..."

while true; do
    STATUS=$(curl -s -k https://localhost:7000/api/archives/$TASK_ID/status | jq -r '.status')
    PROGRESS=$(curl -s -k https://localhost:7000/api/archives/$TASK_ID/status | jq -r '.progress')
    
    echo "Статус: $STATUS ($PROGRESS%)"
    
    if [ "$STATUS" = "Ready" ]; then
        echo "Архив готов!"
        break
    elif [ "$STATUS" = "Failed" ]; then
        echo "Ошибка создания архива!"
        exit 1
    fi
    
    sleep 2
done
```

#### 3.5. Скачивание архива

```bash
# Скачиваем архив
curl -k -O https://localhost:7000/api/archives/$TASK_ID/download

# Проверяем что файл скачался
ls -la archive-*.zip

# Проверяем содержимое архива
unzip -l archive-*.zip

# Ожидаемый результат:
# Archive:  archive-12345678-1234-1234-1234-123456789abc.zip
#   Length      Date    Time    Name
# ---------  ---------- -----   ----
#       25  2025-01-13 12:00   test1.txt
#       25  2025-01-13 12:00   test2.txt
#       18  2025-01-13 12:00   subfolder/nested.txt
# ---------                     -------
#       68                     3 files
```

### Шаг 4: Тестирование через консольный клиент

#### 4.1. Сборка клиента

```bash
cd src/AwesomeFiles.Client
dotnet build
```

#### 4.2. Использование команд

```bash
# Список файлов
dotnet run list

# Создание архива
dotnet run create-archive test1.txt test2.txt

# Проверка статуса (используйте ID из вывода предыдущей команды)
dotnet run status 12345678-1234-1234-1234-123456789abc

# Скачивание архива
dotnet run download 12345678-1234-1234-1234-123456789abc my-archive.zip

# Автоматическое создание и скачивание
dotnet run auto-create-and-download test1.txt test3.txt auto-archive.zip
```

### Шаг 5: Тестирование ошибок

#### 5.1. Несуществующий файл

```bash
curl -k -X POST https://localhost:7000/api/archives \
  -H "Content-Type: application/json" \
  -d '{"files": ["nonexistent.txt"]}' | jq

# Ожидаемый результат:
# {
#   "error": "Files not found",
#   "files": ["nonexistent.txt"]
# }
```

#### 5.2. Path traversal атака

```bash
curl -k -X POST https://localhost:7000/api/archives \
  -H "Content-Type: application/json" \
  -d '{"files": ["../secret.txt"]}' | jq

# Ожидаемый результат:
# {
#   "error": "Invalid file paths",
#   "files": ["../secret.txt"]
# }
```

#### 5.3. Пустой список файлов

```bash
curl -k -X POST https://localhost:7000/api/archives \
  -H "Content-Type: application/json" \
  -d '{"files": []}' | jq

# Ожидаемый результат:
# {
#   "files": [
#     "At least one file must be specified"
#   ]
# }
```

### Шаг 6: Тестирование через Swagger UI

1. Откройте браузер и перейдите по адресу: https://localhost:7000
2. Нажмите "Try it out" для любого endpoint
3. Заполните параметры и нажмите "Execute"
4. Проверьте ответ

### Шаг 7: Тестирование производительности

#### 7.1. Создание больших файлов

```bash
# Создайте файл размером 10MB
dd if=/dev/zero of=files/large-file.bin bs=1M count=10

# Создайте архив с большим файлом
curl -k -X POST https://localhost:7000/api/archives \
  -H "Content-Type: application/json" \
  -d '{"files": ["large-file.bin"]}' | jq
```

#### 7.2. Множественные запросы

```bash
# Создайте несколько архивов одновременно
for i in {1..5}; do
  curl -k -X POST https://localhost:7000/api/archives \
    -H "Content-Type: application/json" \
    -d "{\"files\": [\"test$i.txt\"]}" &
done
wait
```

### Шаг 8: Тестирование через Docker

```bash
# Создайте тестовые файлы для Docker
mkdir -p docker/files
echo "Docker test file" > docker/files/docker-test.txt

# Запустите через Docker Compose
cd docker
docker-compose up --build

# Тестируйте API
curl http://localhost:8080/api/files | jq
```

## Ожидаемые результаты

### Успешный сценарий:
1. ✅ API возвращает список файлов
2. ✅ Создание архива возвращает task ID
3. ✅ Статус меняется: Pending → Processing → Ready
4. ✅ Скачанный ZIP содержит все указанные файлы
5. ✅ Файлы в архиве имеют правильные имена и содержимое

### Обработка ошибок:
1. ✅ Несуществующие файлы → 400 Bad Request
2. ✅ Path traversal → 400 Bad Request
3. ✅ Пустой список файлов → 400 Bad Request
4. ✅ Несуществующий task ID → 404 Not Found
5. ✅ Скачивание неготового архива → 409 Conflict

## Отладка

### Логи API сервера
Следите за логами в терминале где запущен API:
```
info: AwesomeFiles.Api.Controllers.ArchivesController[0]
      Created archive task 12345678-1234-1234-1234-123456789abc for 3 files
info: AwesomeFiles.Api.Background.ArchiveWorker[0]
      Processing archive task 12345678-1234-1234-1234-123456789abc with 3 files
info: AwesomeFiles.Api.Background.ArchiveWorker[0]
      Successfully created archive 12345678-1234-1234-1234-123456789abc at /path/to/archive.zip
```

### Проверка файловой системы
```bash
# Проверьте что архивы создаются
ls -la archives/

# Проверьте временные файлы (должны исчезать)
ls -la archives/*.tmp
```

### Проверка памяти
```bash
# Мониторинг использования памяти
top -p $(pgrep -f "AwesomeFiles.Api")
```
