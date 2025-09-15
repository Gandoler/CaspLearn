# API
## установки и  очень быстрый старт

### docker

> главное указать правильный путь до докер-файла, он лежит `CaspLearn\AwesomeApi\API\AwesomApi\` а запустить откуда угодно: главное чтоб был запушен демон докера

```bash
docker build <path>\CaspLearn\AwesomeApi\API\AwesomApi\    -t awesomeapi

docker run -d -p 5011:8080 --name awesomeapi awesomeapi

```

> и все отлично запустилось)


### обычный (скучный метод)

> переходим в `CaspLearn\AwesomeApi\API\AwesomApi\`

```bash
dotnet build -c Release
dotnet run
```

> и все

### после всего этого заходим на `http://localhost:5011`

## КАРТА ПРОЕКТА бекенда

>попытка сделать чистую архитектуру

```txt
Clear\_system
├── 📄 .dockerignore
│
├── 📂 AwesomApi
│   ├── 📄 AwesomApi.csproj
│   ├── 📄 Program.cs
│   ├── 📄 Dockerfile
│   ├── 📄 appsettings.json
│   ├── 📂 Controllers
│   │   ├── 📄 ArchivesController.cs
│   │   ├── 📄 FilesController.cs
│   │   └── 📂 DTOs
│   │       ├── 📄 ArchiveStatusResponse.cs
│   │       ├── 📄 CreateArchiveRequest.cs
│   │       └── 📄 CreateArchiveResponse.cs
│   ├── 📂 Middleware
│   │   └── 📄 RequestLoggingMiddleware.cs
│   └── 📂 Properties
│       └── 📄 launchSettings.json
│
├── 📂 Model\_Level
│   ├── 📄 Model\_Level.csproj
│   └── 📂 src
│       ├── 📄 ArchiveTask.cs
│       └── 📄 FileInfo.cs
│
├── 📂 Service\_level
│   ├── 📄 Service\_level.csproj
│   ├── 📂 BackgroundServices
│   │   └── 📄 ArchiveWorker.cs
│   └── 📂 Services
│       ├── 📄 ArchiveService.cs
│       ├── 📄 FileListService.cs
│       ├── 📄 IArchiveService.cs
│       └── 📄 IFileListService.cs
│
└── 📂 TESTS
├── 📄 ArchivesControllerTests.cs
├── 📄 FileListServiceTests.cs
└── 📄 TESTS.csproj
```
