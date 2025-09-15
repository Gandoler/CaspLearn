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
dotnet .\Client\bin\Debug\net9.0\Client.dll list
dotnet .\Client\bin\Debug\net9.0\Client.dll list -u http://localhost:5000
```

---

##  Create Archive

Создание архива из файлов:

```powershell
dotnet .\Client\bin\Debug\net9.0\Client.dll create-archive "test1.txt", "test2.txt"
dotnet .\Client\bin\Debug\net9.0\Client.dll create-archive "test1.txt", "test2.txt" -u http://localhost:5000
```

---

##  Status

Проверка статуса архива по ID:

```powershell
dotnet .\Client\bin\Debug\net9.0\Client.dll status <GUID>
dotnet .\Client\bin\Debug\net9.0\Client.dll status <GUID> -u http://localhost:5000
```

---

##  Download

Скачивание архива по ID в указанный путь:

```powershell
dotnet .\Client\bin\Debug\net9.0\Client.dll download <GUID> "C:\Temp\Downloaded.zip"
dotnet .\Client\bin\Debug\net9.0\Client.dll download <GUID> "C:\Temp\Downloaded.zip" -u http://localhost:5000
```

---

##  Auto Archive

Автоматическое создание архива и скачивание (например, из папки):

```powershell
 .\Client\bin\Release\net9.0\Client.dll auto-archive "test1.txt", "test2.txt" --output "C:\Temp\Archive.zip"
 .\Client\bin\Release\net9.0\Client.dll auto-archive "test1.txt", "test2.txt" --output "C:\Temp\Archive.zip" -u http://localhost:5000
```

---
