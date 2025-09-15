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
