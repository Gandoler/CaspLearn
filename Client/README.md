# Client Guide (Перед этим обязательно запустить сервер)

# Запуск консоли



>! очень важно заметить что по-дефолту клиент и сервер настроены на `http://localhost:5011`

переход в дерикторию `....\Client\Client`

```bash
dotnet build  -c Debug
```

>можно и нужно Debug заменить на Release, тогда просто не забыть чуть поменять путь

Вывод после успешной сборки:

```bash
Client успешно выполнено → \bin\Debug\net9.0\Client.dll
```

> тут как раз можно взять путь для следующей команды


```bash
dotnet .\bin\Debug\net9.0\Client.dll --help
```

## Команды

! прошу вас поменяйте путь к dll

Список всех доступных архивов:

```bash
dotnet .\bin\Debug\net9.0\Client.dll list
dotnet .\bin\Debug\net9.0\Client.dll list -u http://localhost:5000
```

---

##  Create Archive

Создание архива из файлов:

```bash
dotnet .\bin\Debug\net9.0\Client.dll create-archive "test1.txt", "test2.txt"
dotnet .\bin\Debug\net9.0\Client.dll create-archive "test1.txt", "test2.txt" -u http://localhost:5000
```

---

##  Status

Проверка статуса архива по ID:

```bash
dotnet .\bin\Debug\net9.0\Client.dll status <GUID>
dotnet .\bin\Debug\net9.0\Client.dll status <GUID> -u http://localhost:5000
```

---

##  Download

Скачивание архива по ID в указанный путь:

```bash
dotnet .\bin\Debug\net9.0\Client.dll download <GUID> "C:\Temp\Downloaded.zip"
dotnet .\bin\Debug\net9.0\Client.dll download <GUID> "C:\Temp\Downloaded.zip" -u http://localhost:5000
```

---

##  Auto Archive

Автоматическое создание архива и скачивание (например, из папки): (нужно путь именно на файл .zip)

```bash
 .\bin\Debug\net9.0\Client.dll auto-archive "test1.txt", "test2.txt" --output "C:\Temp\Archive.zip"
 .\bin\Debug\net9.0\Client.dll auto-archive "test1.txt", "test2.txt" --output "C:\Temp\Archive.zip" -u http://localhost:5000
```

---

## КАРТА ПРОЕКТА клиента


```txt
AwesomeFiles.Client
├── 📄 AwesomeFiles.Client.sln
├── 📄 README.md
│
├── 📂 Client
│   ├── 📄 Client.csproj
│   ├── 📄 Program.cs
│
├── 📂 CommandsServices
│   ├── 📄 CommandsServices.csproj
│   ├── 📂 Commands
│   │   ├── 📄 AutoArchiveCommand.cs
│   │   ├── 📄 CreateArchiveCommand.cs
│   │   ├── 📄 DownloadCommand.cs
│   │   ├── 📄 ListCommand.cs
│   │   └── 📄 StatusCommand.cs
│   └── 📂 Services
│       ├── 📄 ApiClient.cs
│       └── 📄 IApiClient.cs
│
├── 📂 ModelLevel
│   ├── 📄 ModelLevel.csproj
│   └── 📂 Models
│       ├── 📄 ArchiveStatus.cs
│       ├── 📄 ClientModels.cs
│       └── 📄 FileInfo.cs
│
└── 📂 TESTS_CLIENT
    ├── 📄 TESTS_CLIENT.csproj
    ├── 📂 Commands
    │   ├── 📄 AutoArchiveCommandTests.cs
```
