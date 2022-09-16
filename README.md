# UNAHUR Message-Fun

Proyecto de ejemplo para la asignatura Arquitectura de Software II de la Universidad de Hurlingham

## RUN

```bash

# DOCKER
docker compose up -d
# NERDCTL
nerdctl compose up -d

```


## DEBUG

**Herramientas**

- [Visual Studio 2022 Community](https://visualstudio.microsoft.com/es/vs/community/)
- Rancher Desktop
- Instancia de Rabbit (con el siguiente comando)

```bash
# DOCKER
docker run -d --hostname myrabbit --name rabbitmq -p 8080:15672 -p 5672:5672 -e RABBITMQ_DEFAULT_USER=desa -e RABBITMQ_DEFAULT_PASS=desarrollo masstransit/rabbitmq:latest
# NERDCRL
nerdctl run -d --hostname myrabbit --name rabbitmq -p 8080:15672 -p 5672:5672 -e RABBITMQ_DEFAULT_USER=desa -e RABBITMQ_DEFAULT_PASS=desarrollo masstransit/rabbitmq:latest
```

**Limpieza**
Para eliminar el contenedor creado
```bash
# DOCKER
docker stop myrabbit
docker rm myrabbit

# NERDCTL

nerdctl rm myrabbit
```

