# Быстрый старт Awesome Files API

## 🚀 Запуск за 5 минут

### 1. Подготовка (1 минута)

```bash
# Убедитесь что у вас установлен .NET 8 SDK
dotnet --version

# Перейдите в папку проекта
cd AwesomeApi

# Создайте тестовые файлы (уже созданы)
ls files/
# Должны увидеть: test1.txt, test2.txt, subfolder/nested.txt
```

### 2. Запуск API сервера (1 минута)

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

### 3. Тестирование API (2 минуты)

#### Через браузер (Swagger UI):
1. Откройте: https://localhost:7000
2. Нажмите "Try it out" для `/api/files`
3. Нажмите "Execute"
4. Увидите список файлов

#### Через curl:
```bash
# В другом терминале
curl -k https://localhost:7000/api/files | jq

# Создать архив
curl -k -X POST https://localhost:7000/api/archives \
  -H "Content-Type: application/json" \
  -d '{"files": ["test1.txt", "test2.txt"]}' | jq

# Сохраните ID из ответа и проверьте статус
TASK_ID="ваш-id-здесь"
curl -k https://localhost:7000/api/archives/$TASK_ID/status | jq

# Скачайте когда готов
curl -k -O https://localhost:7000/api/archives/$TASK_ID/download
```

### 4. Тестирование клиента (1 минута)

```bash
# В третьем терминале
cd src/AwesomeFiles.Client

# Список файлов
dotnet run list

# Автоматическое создание и скачивание
dotnet run auto-create-and-download test1.txt test2.txt my-archive.zip

# Проверьте что архив создался
ls -la my-archive.zip
unzip -l my-archive.zip
```

## 🐳 Альтернатива: Docker

```bash
# Запуск через Docker Compose
cd docker
docker-compose up --build

# API будет доступен по адресу: http://localhost:8080
curl http://localhost:8080/api/files | jq
```

## 🧪 Запуск тестов

```bash
# Все тесты
dotnet test

# Только тесты API
dotnet test tests/AwesomeFiles.Api.Tests/
```

## 📋 Что должно работать

### ✅ Успешные сценарии:
1. **Список файлов** - GET `/api/files` возвращает JSON с файлами
2. **Создание архива** - POST `/api/archives` возвращает task ID
3. **Проверка статуса** - GET `/api/archives/{id}/status` показывает прогресс
4. **Скачивание** - GET `/api/archives/{id}/download` возвращает ZIP файл
5. **Клиент** - все команды работают корректно

### ❌ Обработка ошибок:
1. **Несуществующий файл** - 400 Bad Request
2. **Path traversal** - 400 Bad Request  
3. **Пустой список** - 400 Bad Request
4. **Несуществующий task** - 404 Not Found

## 🔧 Настройка

### Переменные окружения:
```bash
export FILES_ROOT="/path/to/your/files"
export ARCHIVES_DIR="/path/to/archives"
```

### Конфигурация в appsettings.json:
```json
{
  "FILES_ROOT": "./files",
  "ARCHIVES_DIR": "./archives"
}
```

## 🐛 Troubleshooting

### Проблема: "Connection refused"
```bash
# Проверьте что API запущен
curl -k https://localhost:7000/health

# Если не работает, проверьте порт
netstat -an | grep 7000
```

### Проблема: "File not found"
```bash
# Проверьте что файлы существуют
ls -la files/

# Проверьте права доступа
chmod 755 files/
```

### Проблема: "SSL certificate error"
```bash
# Используйте -k флаг с curl
curl -k https://localhost:7000/api/files

# Или отключите HTTPS в launchSettings.json
```

## 📚 Дополнительные ресурсы

- [README.md](README.md) - Полная документация
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - Подробное руководство по тестированию
- [QUICK_TEST.md](QUICK_TEST.md) - Быстрые команды для тестирования
- [EXTENSION_POINTS.md](EXTENSION_POINTS.md) - Точки расширения функциональности

## 🎯 Следующие шаги

1. **Изучите код** - начните с `Program.cs` и контроллеров
2. **Добавьте свои файлы** - поместите файлы в папку `files/`
3. **Настройте окружение** - измените переменные окружения
4. **Расширьте функциональность** - используйте точки расширения
5. **Добавьте тесты** - создайте свои тестовые сценарии

## 💡 Полезные команды

```bash
# Просмотр логов API
tail -f logs/app.log

# Мониторинг использования памяти
top -p $(pgrep -f "AwesomeFiles.Api")

# Проверка портов
lsof -i :7000

# Очистка архивов
rm -rf archives/*.zip
```

Удачи в использовании Awesome Files API! 🚀
