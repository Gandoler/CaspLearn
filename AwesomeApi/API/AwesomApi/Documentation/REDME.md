# API




## установки

### docker



```bash
docker build -t awesomeapi --build-arg EXTRA_PATH=./E_API  <path>



docker run -d -p 5010:8080 -v E:/API/files:/app/files  -v E:/API/archives:/app/archives -v E:/API/logs:/app/logs --name awesomeapi awesomeapi
```

