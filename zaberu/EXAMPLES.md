# Примеры использования Awesome Files Client

## Базовые сценарии

### 1. Просмотр доступных файлов
```bash
# Показать все файлы
dotnet util.dll list

# Вывод:
# Found 4 files:
# 
#   file1.txt (1.2 KB) - Modified: 2024-01-15 10:30:45
#   file2.txt (2.5 KB) - Modified: 2024-01-15 11:15:20
#   image.png (150.3 KB) - Modified: 2024-01-15 09:45:12
#   document.pdf (1.2 MB) - Modified: 2024-01-15 12:00:00
```

### 2. Создание архива пошагово
```bash
# Шаг 1: Создать архив
dotnet util.dll create-archive file1.txt file2.txt image.png
# Вывод: Create archive task is started, id: 12345678-1234-1234-1234-123456789abc

# Шаг 2: Проверить статус
dotnet util.dll status 12345678-1234-1234-1234-123456789abc
# Вывод: Process in progress, please wait… (25%)

# Шаг 3: Проверить статус еще раз
dotnet util.dll status 12345678-1234-1234-1234-123456789abc
# Вывод: Archive has been created.

# Шаг 4: Скачать архив
dotnet util.dll download 12345678-1234-1234-1234-123456789abc ./downloads/my-archive.zip
# Вывод: Archive downloaded successfully to: ./downloads/my-archive.zip
```

### 3. Автоматический режим
```bash
# Создать и скачать архив одной командой
dotnet util.dll auto-archive file1.txt file2.txt --output ./my-archive.zip

# Вывод:
# Creating archive for 2 files...
# Archive task created with ID: 12345678-1234-1234-1234-123456789abc
# Waiting for archive creation to complete...
# Processing... 25%
# Processing... 50%
# Processing... 75%
# Processing... 100%
# Archive is ready!
# Downloading archive to: ./my-archive.zip
# Archive downloaded successfully to: ./my-archive.zip
```

## Продвинутые сценарии

### 4. Работа с удаленным сервером
```bash
# Использование с удаленным API
dotnet util.dll --base-url https://api.mycompany.com:8080 list
dotnet util.dll --base-url https://api.mycompany.com:8080 create-archive file1.txt
```

### 5. Настройка автоматического режима
```bash
# Быстрый опрос (каждую секунду) с длинным таймаутом
dotnet util.dll auto-archive file1.txt file2.txt \
  --output ./archive.zip \
  --poll-interval 1000 \
  --timeout 600000

# Медленный опрос (каждые 5 секунд) с коротким таймаутом
dotnet util.dll auto-archive file1.txt file2.txt \
  --output ./archive.zip \
  --poll-interval 5000 \
  --timeout 120000
```

### 6. Пакетная обработка
```bash
#!/bin/bash
# Скрипт для создания нескольких архивов

files1=("file1.txt" "file2.txt")
files2=("image1.png" "image2.png" "image3.png")
files3=("document1.pdf" "document2.pdf")

# Создать архив текстовых файлов
dotnet util.dll auto-archive "${files1[@]}" --output ./archives/text-files.zip

# Создать архив изображений
dotnet util.dll auto-archive "${files2[@]}" --output ./archives/images.zip

# Создать архив документов
dotnet util.dll auto-archive "${files3[@]}" --output ./archives/documents.zip
```

## Обработка ошибок

### 7. Ошибки API
```bash
# Попытка создать архив из несуществующих файлов
dotnet util.dll create-archive nonexistent.txt
# Вывод: Error: Files not found

# Попытка скачать несуществующий архив
dotnet util.dll download 00000000-0000-0000-0000-000000000000 ./test.zip
# Вывод: Error: Archive task not found

# Попытка скачать архив, который еще не готов
dotnet util.dll download 12345678-1234-1234-1234-123456789abc ./test.zip
# Вывод: Error: Archive is not ready
```

### 8. Ошибки сети
```bash
# Недоступный сервер
dotnet util.dll --base-url http://nonexistent-server:5010 list
# Вывод: Error: Network error: No such host is known

# Неверный порт
dotnet util.dll --base-url http://localhost:9999 list
# Вывод: Error: Network error: Connection refused
```

### 9. Ошибки валидации
```bash
# Неверный формат ID
dotnet util.dll status invalid-id
# Вывод: Error: Invalid archive ID format

# Неверный путь для сохранения
dotnet util.dll download 12345678-1234-1234-1234-123456789abc /invalid/path/archive.zip
# Вывод: Error: Could not find a part of the path '/invalid/path'
```

## Интеграция в скрипты

### 10. PowerShell скрипт
```powershell
# Создать архив и проверить результат
$files = @("file1.txt", "file2.txt")
$output = "./archive.zip"

Write-Host "Creating archive..."
$result = dotnet util.dll auto-archive $files --output $output

if ($LASTEXITCODE -eq 0) {
    Write-Host "Archive created successfully: $output"
    $fileInfo = Get-Item $output
    Write-Host "File size: $($fileInfo.Length) bytes"
} else {
    Write-Host "Failed to create archive"
    exit 1
}
```

### 11. Bash скрипт с проверками
```bash
#!/bin/bash

# Функция для создания архива с проверками
create_archive() {
    local files=("$@")
    local output="./archive-$(date +%Y%m%d-%H%M%S).zip"
    
    echo "Creating archive with ${#files[@]} files..."
    
    if dotnet util.dll auto-archive "${files[@]}" --output "$output"; then
        echo "✅ Archive created successfully: $output"
        ls -lh "$output"
        return 0
    else
        echo "❌ Failed to create archive"
        return 1
    fi
}

# Использование
create_archive "file1.txt" "file2.txt" "image.png"
```

### 12. Мониторинг статуса архива
```bash
#!/bin/bash

# Создать архив и мониторить его статус
ARCHIVE_ID=$(dotnet util.dll create-archive file1.txt file2.txt | grep -o '[0-9a-f-]\{36\}')

if [ -z "$ARCHIVE_ID" ]; then
    echo "Failed to create archive"
    exit 1
fi

echo "Archive ID: $ARCHIVE_ID"

# Мониторить статус каждые 2 секунды
while true; do
    STATUS=$(dotnet util.dll status "$ARCHIVE_ID")
    echo "$(date): $STATUS"
    
    if [[ "$STATUS" == *"has been created"* ]]; then
        echo "Archive is ready! Downloading..."
        dotnet util.dll download "$ARCHIVE_ID" "./archive-$ARCHIVE_ID.zip"
        break
    elif [[ "$STATUS" == *"failed"* ]]; then
        echo "Archive creation failed!"
        exit 1
    fi
    
    sleep 2
done
```

## Лучшие практики

### 13. Использование в CI/CD
```yaml
# GitHub Actions пример
- name: Create archive
  run: |
    dotnet util.dll auto-archive \
      --output ./artifacts/build-archive.zip \
      --timeout 300000 \
      file1.txt file2.txt

- name: Upload archive
  uses: actions/upload-artifact@v3
  with:
    name: build-archive
    path: ./artifacts/build-archive.zip
```

### 14. Обработка больших файлов
```bash
# Для больших архивов используйте длинный таймаут
dotnet util.dll auto-archive large-file1.zip large-file2.zip \
  --output ./combined-archive.zip \
  --timeout 1800000 \
  --poll-interval 5000
```

### 15. Логирование и отладка
```bash
# Включить подробное логирование (если настроено)
export DOTNET_LOG_LEVEL=Debug
dotnet util.dll auto-archive file1.txt --output ./archive.zip
```
