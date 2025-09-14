# Awesome Files Client

Консольная утилита для работы с Awesome Files API - сервисом создания ZIP архивов из файлов.

## Возможности

- 📁 Получение списка доступных файлов
- 📦 Создание архивов из выбранных файлов
- 📊 Проверка статуса создания архива
- ⬇️ Скачивание готовых архивов
- 🤖 Автоматический режим (создание + ожидание + скачивание)
- 🎯 Поддержка POSIX стандарта для CLI
- ✅ Обработка ошибок от backend сервиса

## Установка

1. Убедитесь, что у вас установлен .NET 9.0 или выше
2. Соберите проект:
   ```bash
   dotnet build Client/AwesomeFiles.Client.csproj
   ```

## Использование

### Базовые команды

#### 1. Получение списка файлов
```bash
dotnet util.dll list
```

Выводит список всех доступных файлов с их размерами и датами изменения.

#### 2. Создание архива
```bash
dotnet util.dll create-archive file1.txt file2.txt file3.txt
```

Создает задачу архивирования для указанных файлов. Возвращает ID задачи.

#### 3. Проверка статуса архива
```bash
dotnet util.dll status <archive-id>
```

Показывает текущий статус создания архива:
- `Pending` - задача в очереди
- `Processing` - архив создается (с процентами)
- `Ready` - архив готов к скачиванию
- `Failed` - ошибка создания архива

#### 4. Скачивание архива
```bash
dotnet util.dll download <archive-id> /path/to/save/archive.zip
```

Скачивает готовый архив в указанную папку.

### Автоматический режим

Для автоматического создания архива, ожидания его готовности и скачивания используйте команду `auto-archive`:

```bash
dotnet util.dll auto-archive file1.txt file2.txt --output /path/to/archive.zip
```

#### Опции автоматического режима:
- `--output, -o` - путь для сохранения архива (обязательно)
- `--poll-interval, -i` - интервал опроса статуса в миллисекундах (по умолчанию: 2000)
- `--timeout, -t` - таймаут в миллисекундах (по умолчанию: 300000 = 5 минут)

Пример с настройками:
```bash
dotnet util.dll auto-archive file1.txt file2.txt --output /tmp/my-archive.zip --poll-interval 1000 --timeout 600000
```

### Глобальные опции

- `--base-url, -u` - базовый URL API сервера (по умолчанию: http://localhost:5010)

Пример:
```bash
dotnet util.dll --base-url http://api.example.com:8080 list
```

## Примеры использования

### Пример 1: Базовый workflow
```bash
# 1. Посмотреть доступные файлы
dotnet util.dll list

# 2. Создать архив
dotnet util.dll create-archive file1.txt file2.txt
# Вывод: Create archive task is started, id: 291932

# 3. Проверить статус
dotnet util.dll status 291932
# Вывод: Process in progress, please wait… (50%)

# 4. Проверить статус еще раз
dotnet util.dll status 291932
# Вывод: Archive has been created.

# 5. Скачать архив
dotnet util.dll download 291932 ./downloads/archive.zip
# Вывод: Archive downloaded successfully to: ./downloads/archive.zip
```

### Пример 2: Автоматический режим
```bash
# Создать и скачать архив одной командой
dotnet util.dll auto-archive file1.txt file2.txt --output ./my-archive.zip
# Вывод:
# Creating archive for 2 files...
# Archive task created with ID: 291932
# Waiting for archive creation to complete...
# Processing... 50%
# Processing... 100%
# Archive is ready!
# Downloading archive to: ./my-archive.zip
# Archive downloaded successfully to: ./my-archive.zip
```

### Пример 3: Работа с удаленным сервером
```bash
# Использование с удаленным API сервером
dotnet util.dll --base-url https://api.mycompany.com list
dotnet util.dll --base-url https://api.mycompany.com create-archive file1.txt
```

## Обработка ошибок

Утилита обрабатывает различные типы ошибок:

### Ошибки API
- **400 Bad Request** - неверные параметры запроса
- **404 Not Found** - файл или архив не найден
- **409 Conflict** - архив еще не готов к скачиванию
- **500 Internal Server Error** - внутренняя ошибка сервера

### Ошибки сети
- Недоступность API сервера
- Таймауты соединения
- Проблемы с DNS

### Ошибки валидации
- Неверный формат ID архива
- Несуществующие пути для сохранения файлов
- Отсутствие обязательных параметров

## Коды возврата

- `0` - успешное выполнение
- `1` - ошибка выполнения

## Конфигурация

Утилита использует следующие настройки по умолчанию:
- Базовый URL API: `http://localhost:5010`
- Таймаут HTTP запросов: 10 минут
- Интервал опроса статуса: 2 секунды
- Таймаут автоматического режима: 5 минут

## Требования

- .NET 9.0 или выше
- Доступ к API серверу Awesome Files
- Права на запись в папку назначения для скачивания архивов

## Поддержка

При возникновении проблем проверьте:
1. Доступность API сервера
2. Правильность URL и порта
3. Существование указанных файлов
4. Права на запись в папку назначения
5. Логи сервера для диагностики ошибок
