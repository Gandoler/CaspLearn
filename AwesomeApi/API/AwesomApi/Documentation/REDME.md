# API




## установки

### установка логов на сервер 

```bash
sudo apt update
sudo apt install ufw -y
sudo ufw enable
sudo ufw allow 5341/tcp

docker run --name seq -d -e ACCEPT_EULA=Y -p 5341:5341 datalust/seq
sudo ufw allow 5341/tcp


```

