# Awesome Files API

REST API для создания ZIP-архивов из файлов с консольным клиентом.

## Возможности

- 📁 Просмотр списка доступных файлов
- 📦 Создание ZIP-архивов из выбранных файлов
- ⏳ Асинхронная обработка с отслеживанием статуса
- 🔒 Валидация путей файлов (защита от path traversal)
- 🚀 Консольный клиент с удобными командами
- 🐳 Docker поддержка
- 📊 Swagger документация
- 🧪 Unit тесты

## Технологии

- **Backend**: ASP.NET Core Web API (.NET 8)
- **Клиент**: .NET Console App
- **Тесты**: xUnit
- **Документация**: Swagger/OpenAPI
- **Контейнеризация**: Docker

## Структура проекта

```
src/
├── AwesomeFiles.Api/          # Web API проект
│   ├── Controllers/           # API контроллеры
│   ├── Services/             # Бизнес-логика
│   ├── Background/           # Фоновые сервисы
│   ├── Middleware/           # Промежуточное ПО
│   └── Models/               # Модели данных
├── AwesomeFiles.Client/       # Консольный клиент
│   └── Commands/             # CLI команды
└── AwesomeFiles.Common/       # Общие модели и DTOs

tests/
├── AwesomeFiles.Api.Tests/    # Тесты API
└── AwesomeFiles.Client.Tests/ # Тесты клиента

docker/
├── Dockerfile                # Docker образ
└── docker-compose.yml        # Docker Compose
```

## Быстрый старт

### 1. Настройка окружения

```bash
# Клонируйте репозиторий
git clone <repository-url>
cd AwesomeApi

# Создайте папку для тестовых файлов
mkdir -p files
echo "Hello World!" > files/test1.txt
echo "Another file" > files/test2.txt
mkdir -p files/subfolder
echo "Nested file" > files/subfolder/test3.txt
```

### 2. Запуск локально

```bash
# Запуск API
cd src/AwesomeFiles.Api
dotnet run

# В другом терминале - запуск клиента
cd src/AwesomeFiles.Client
dotnet run --help
```

### 3. Запуск через Docker

```bash
# Создайте тестовые файлы
mkdir -p docker/files
echo "Docker test file" > docker/files/docker-test.txt

# Запуск через Docker Compose
cd docker
docker-compose up --build
```

## API Endpoints

### GET /api/files
Получить список всех доступных файлов.

**Response:**
```json
[
  {
    "name": "test1.txt",
    "size": 12,
    "modified": "2025-01-13T12:00:00Z"
  }
]
```

### POST /api/archives
Создать новый архив.

**Request:**
```json
{
  "files": ["test1.txt", "subfolder/test3.txt"]
}
```

**Response:**
```json
{
  "id": "12345678-1234-1234-1234-123456789abc"
}
```

### GET /api/archives/{id}/status
Получить статус архива.

**Response:**
```json
{
  "id": "12345678-1234-1234-1234-123456789abc",
  "status": "Processing",
  "progress": 50,
  "message": "Processing file 1/2"
}
```

**Статусы:**
- `Pending` - Задача в очереди
- `Processing` - Архив создается
- `Ready` - Архив готов к скачиванию
- `Failed` - Ошибка создания архива

### GET /api/archives/{id}/download
Скачать готовый архив.

**Response:** ZIP файл (application/zip)

## Консольный клиент

### Команды

```bash
# Показать справку
dotnet run --help

# Список доступных файлов
dotnet run list

# Создать архив
dotnet run create-archive file1.txt file2.txt

# Проверить статус
dotnet run status <task-id>

# Скачать архив
dotnet run download <task-id> archive.zip

# Автоматически создать и скачать
dotnet run auto-create-and-download file1.txt file2.txt archive.zip
```

### Примеры использования

```bash
# 1. Посмотреть доступные файлы
dotnet run list

# 2. Создать архив
dotnet run create-archive test1.txt subfolder/test3.txt

# 3. Проверить статус (используйте ID из предыдущей команды)
dotnet run status 12345678-1234-1234-1234-123456789abc

# 4. Скачать когда готов
dotnet run download 12345678-1234-1234-1234-123456789abc my-archive.zip

# Или все в одной команде:
dotnet run auto-create-and-download test1.txt test2.txt my-archive.zip
```

## Переменные окружения

| Переменная | Описание | По умолчанию |
|------------|----------|--------------|
| `FILES_ROOT` | Папка с исходными файлами | `./files` |
| `ARCHIVES_DIR` | Папка для сохранения архивов | `./archives` |
| `ASPNETCORE_ENVIRONMENT` | Окружение ASP.NET Core | `Development` |

## Тестирование

### Запуск тестов

```bash
# Все тесты
dotnet test

# Только тесты API
dotnet test tests/AwesomeFiles.Api.Tests/

# С покрытием кода
dotnet test --collect:"XPlat Code Coverage"
```

### Тестовые сценарии

1. **Валидация путей файлов:**
   ```bash
   curl -X POST http://localhost:5000/api/archives \
     -H "Content-Type: application/json" \
     -d '{"files": ["../secret.txt"]}'
   # Ожидается: 400 Bad Request
   ```

2. **Полный цикл создания архива:**
   ```bash
   # 1. Создать архив
   curl -X POST http://localhost:5000/api/archives \
     -H "Content-Type: application/json" \
     -d '{"files": ["test1.txt", "test2.txt"]}'
   
   # 2. Проверить статус (заменить ID)
   curl http://localhost:5000/api/archives/{id}/status
   
   # 3. Скачать когда готов
   curl -O http://localhost:5000/api/archives/{id}/download
   ```

## Docker

### Сборка образа

```bash
docker build -f docker/Dockerfile -t awesome-files-api .
```

### Запуск контейнера

```bash
docker run -p 8080:8080 \
  -v $(pwd)/files:/app/files:ro \
  -v $(pwd)/archives:/app/archives \
  awesome-files-api
```

### Docker Compose

```bash
cd docker
docker-compose up --build
```

API будет доступен по адресу: http://localhost:8080

## Разработка

### Добавление новых функций

1. **Кэширование архивов** - TODO в `ArchiveWorker.cs`
2. **Персистентное хранение задач** - TODO в `ArchiveService.cs`
3. **Логирование в БД** - TODO в `Program.cs`
4. **Аутентификация** - TODO в `Program.cs`

### Архитектурные решения

- **In-memory хранилище задач** - для простоты, легко заменить на Redis/БД
- **Channel-based очередь** - эффективная обработка задач
- **Потоковая архивация** - не загружает все файлы в память
- **Атомарные операции** - временные файлы переименовываются атомарно

## Troubleshooting

### Проблемы с путями файлов

```bash
# Проверьте права доступа
ls -la files/

# Убедитесь что файлы существуют
find files/ -type f
```

### Проблемы с Docker

```bash
# Проверьте логи
docker-compose logs awesome-files-api

# Пересоберите образ
docker-compose build --no-cache
```

### Проблемы с клиентом

```bash
# Проверьте URL сервера
dotnet run list --server http://localhost:8080

# Проверьте доступность API
curl http://localhost:8080/health
```

## Лицензия

MIT License

## Вклад в проект

1. Fork репозитория
2. Создайте feature branch
3. Добавьте тесты для новой функциональности
4. Убедитесь что все тесты проходят
5. Создайте Pull Request
