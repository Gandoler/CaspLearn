# Быстрый старт - Awesome Files Client

## 🚀 Быстрая установка и запуск

### 1. Сборка проекта
```bash
# Собрать все проекты
dotnet build

# Или только клиент
dotnet build Client/AwesomeFiles.Client.csproj
```

### 2. Запуск тестов
```bash
# Запустить все тесты
dotnet test

# Или только тесты клиента
dotnet test Client.Tests/AwesomeFiles.Client.Tests.csproj
```

### 3. Быстрое тестирование
```bash
# Запустить скрипт тестирования
./test-client.sh
```

## 📋 Основные команды

### Просмотр справки
```bash
dotnet run --project Client/AwesomeFiles.Client.csproj -- --help
```

### Список файлов
```bash
dotnet run --project Client/AwesomeFiles.Client.csproj -- list
```

### Создание архива
```bash
dotnet run --project Client/AwesomeFiles.Client.csproj -- create-archive file1.txt file2.txt
```

### Проверка статуса
```bash
dotnet run --project Client/AwesomeFiles.Client.csproj -- status <archive-id>
```

### Скачивание архива
```bash
dotnet run --project Client/AwesomeFiles.Client.csproj -- download <archive-id> ./archive.zip
```

### Автоматический режим
```bash
dotnet run --project Client/AwesomeFiles.Client.csproj -- auto-archive file1.txt file2.txt --output ./archive.zip
```

## 🔧 Сборка исполняемого файла

### Для разработки
```bash
dotnet publish Client/AwesomeFiles.Client.csproj --configuration Release --self-contained false
```

### Для развертывания
```bash
dotnet publish Client/AwesomeFiles.Client.csproj --configuration Release --self-contained true --runtime linux-x64
```

После сборки исполняемый файл будет находиться в папке `Client/bin/Release/net9.0/publish/`.

## 🌐 Работа с удаленным API

```bash
# Указать другой URL API сервера
dotnet run --project Client/AwesomeFiles.Client.csproj -- --base-url http://api.example.com:8080 list
```

## 📁 Структура проекта

```
Client/
├── Commands/           # Команды CLI
├── Models/            # Модели данных
├── Services/          # Сервисы для работы с API
├── Program.cs         # Точка входа
└── README.md          # Подробная документация

Client.Tests/
├── Commands/          # Тесты команд
└── Services/          # Тесты сервисов
```

## ⚠️ Требования

- .NET 9.0 или выше
- Доступ к API серверу Awesome Files (по умолчанию http://localhost:5010)
- Права на запись в папку назначения для скачивания архивов

## 🆘 Решение проблем

### Ошибка "API server not available"
- Убедитесь, что API сервер запущен
- Проверьте URL и порт: `--base-url http://localhost:5010`

### Ошибка "Files not found"
- Проверьте, что указанные файлы существуют на сервере
- Используйте команду `list` для просмотра доступных файлов

### Ошибка "Archive is not ready"
- Дождитесь завершения создания архива
- Используйте команду `status` для проверки прогресса

## 📚 Дополнительная документация

- [README.md](Client/README.md) - Подробная документация
- [EXAMPLES.md](Client/EXAMPLES.md) - Примеры использования
- [CHANGELOG.md](Client/CHANGELOG.md) - История изменений
