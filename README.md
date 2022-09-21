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

## Contenedor RabbitMQ

Broker de mensajes para la comunicación entre servicios

## Endpoints



## Configuración



## Contenedor API

Recibe comando vía REST. Los encola en el bus de mensajes y envía la respuesta pertinente.

[![Docker Repository on Quay](https://quay.io/repository/unahur.arqsw/messagefun.api/status "Docker Repository on Quay")](https://quay.io/repository/unahur.arqsw/messagefun.api)

### Endpoints

| NOMBRE     | PUERTO | PATH            | DESCRIPCION                           |
| ---------- | ------ | --------------- | ------------------------------------- |
| API        | 8080   | `/api`          | Apis REST                             |
| SWAGGER-UI | 8080   | `/swagger`      | Interface HTML de prueba (Swagger-UI) |
| METRICAS   | 8080   | `/metrics`      | Métricas en formato *Prometheus*      |
| HEALTH     | 8080   | `/healthz/live`  | Sonda de servicio VIVO                |
| READY      | 8080   | `/healthz/ready` | Sonda de servicio LISTO               |

### Configuración

El sistema puede configurarse mediante un archivo `app\appsettings.json`) o variables de entorno

```json
{
  "MessageBusFun": {
    "RabbitMQ": {
      "Host": "rabbitmq://rabbitmq:5672",
      "Username": "desa",
      "Password": "desarrollo"
    }
  }
}
```

| PATH                              | ENV                                | DESCRIPCION                      |
| --------------------------------- | ---------------------------------- | -------------------------------- |
| `MessageBusFun.RabbitMQ.Host`     | `MessageBusFun__RabbitMQ__Host`    | URI de Rabbit MQ                 |
| `MessageBusFun.RabbitMQ.Username` | MessageBusFun__RabbitMQ__Username` | Nombre de usuario de RabbitMQ    |
| `MessageBusFun.RabbitMQ.Password` | MessageBusFun__RabbitMQ__Password` | Password del usuario de RabbitMQ |

## Contenedor Worker

Procesa trabajos en segundo plano

[![Docker Repository on Quay](https://quay.io/repository/unahur.arqsw/messagefun.worker/status "Docker Repository on Quay")](https://quay.io/repository/unahur.arqsw/messagefun.worker)

### Endpoints

| NOMBRE     | PUERTO | PATH            | DESCRIPCION                           |
| ---------- | ------ | --------------- | ------------------------------------- |
| METRICAS   | 9090   | `/metrics`      | Métricas en formato *Prometheus*      |
| HEALTH     | 9090   | `/healthz/live`  | Sonda de servicio VIVO                |
| READY      | 9090   | `/healthz/ready` | Sonda de servicio LISTO               |

### Configuración

IDEM API
