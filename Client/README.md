# Client Guide (Перед этим обязательно запустить сервер)

# Запуск консоли



>! очень важно заметить что по-дефолту клиент и сервер настроены на `http://localhost:5011`

переход в дерикторию `.CaspLearn\Client`

```bash
dotnet build .\Client\Client.csproj -c Debug
```

>можно и нужно Debug заменить на Release, тогда просто не забыть чуть поменять путь

Вывод после успешной сборки:

```bash
Client успешно выполнено → Client\bin\Debug\net9.0\Client.dll
```

> тут как раз можно взять путь для следующей команды


```bash
dotnet .\Client\bin\Debug\net9.0\Client.dll --help
```

## Команды


Список всех доступных архивов:

```bash
dotnet .\Client\bin\Debug\net9.0\Client.dll list
dotnet .\Client\bin\Debug\net9.0\Client.dll list -u http://localhost:5000
```

---

##  Create Archive

Создание архива из файлов:

```bash
dotnet .\Client\bin\Debug\net9.0\Client.dll create-archive "test1.txt", "test2.txt"
dotnet .\Client\bin\Debug\net9.0\Client.dll create-archive "test1.txt", "test2.txt" -u http://localhost:5000
```

---

##  Status

Проверка статуса архива по ID:

```bash
dotnet .\Client\bin\Debug\net9.0\Client.dll status <GUID>
dotnet .\Client\bin\Debug\net9.0\Client.dll status <GUID> -u http://localhost:5000
```

---

##  Download

Скачивание архива по ID в указанный путь:

```bash
dotnet .\Client\bin\Debug\net9.0\Client.dll download <GUID> "C:\Temp\Downloaded.zip"
dotnet .\Client\bin\Debug\net9.0\Client.dll download <GUID> "C:\Temp\Downloaded.zip" -u http://localhost:5000
```

---

##  Auto Archive

Автоматическое создание архива и скачивание (например, из папки):

```bash
 .\Client\bin\Debug\net9.0\Client.dll auto-archive "test1.txt", "test2.txt" --output "C:\Temp\Archive.zip"
 .\Client\bin\Debug\net9.0\Client.dll auto-archive "test1.txt", "test2.txt" --output "C:\Temp\Archive.zip" -u http://localhost:5000
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
