## Client Guide

# Запуск консоли

>! очень важно заметить что по-дефолту клиент и сервер настроены на `http://localhost:5011`

переход в дерикторию `.CaspLearn\Client`
```powershell
dotnet build .\Client\Client.csproj -c Debug
```

Вывод после успешной сборки:

```powershell
Client успешно выполнено → Client\bin\Debug\net9.0\Client.dll
```

> тут как раз можно взять путь для следующей команды


```powershell
dotnet .\Client\bin\Debug\net9.0\Client.dll --help 
```

## Команды


Список всех доступных архивов:

```powershell
dotnet .\bin\Debug\net9.0\Client.dll list
dotnet .\bin\Debug\net9.0\Client.dll list -u http://localhost:5000
```

---

##  Create Archive

Создание архива из файлов:

```powershell
dotnet .\bin\Debug\net9.0\Client.dll create-archive "C:\Temp\FilesToArchive"
dotnet .\bin\Debug\net9.0\Client.dll create-archive "C:\Temp\FilesToArchive" -u http://localhost:5000
```

---

##  Status

Проверка статуса архива по ID:

```powershell
dotnet .\bin\Debug\net9.0\Client.dll status 12345
dotnet .\bin\Debug\net9.0\Client.dll status 12345 -u http://localhost:5000
```

---

##  Download

Скачивание архива по ID в указанный путь:

```powershell
dotnet .\bin\Debug\net9.0\Client.dll download 12345 "C:\Temp\Downloaded.zip"
dotnet .\bin\Debug\net9.0\Client.dll download 12345 "C:\Temp\Downloaded.zip" -u http://localhost:5000
```

---

##  Auto Archive

Автоматическое создание архива и скачивание (например, из папки):

```powershell
dotnet .\bin\Debug\net9.0\Client.dll auto-archive "C:\Temp\FilesToArchive"
dotnet .\bin\Debug\net9.0\Client.dll auto-archive "C:\Temp\FilesToArchive" -u http://localhost:5000
```

---