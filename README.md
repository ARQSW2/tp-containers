# UNAHUR Message-Fun

Proyecto de ejemplo para la asignatura Arquitectura de Software II de la Universidad de Hurlingham

![diagrama](./docs/diagram.svg)

## Funcionamiento

La solución consta de tres componentes

- API: recibe comando vía REST. Los encola en el bus de mensajes y envía la respuesta pertinente
  - `POST api/message/{data}`: Crea un proceso batch `IDoJob` a ser procesado por worker
  - `GET api/message/{name}`: Envia el comando `IGreetingCmd` para recibir una respuesta desde el worker
- Worker: Procesa trabajos en segundo plano
  - `DoJobConsumer`: Procesa el job batch `IDoJob`.
  - `GreetingCmdConsumer`: Procesa el job batch `IGreetingCmd`.
- RabbitMQ: bus de mensajes AMQP que conecta **API** y **Worker**

```mermaid
sequenceDiagram
  actor User
  User->>+API: POST api/message/payload
  API->>RabbitMQ: IDoJob + payload
  API-->>-User:HTTP 200 JobId
  RabbitMQ->>Worker: DoJobConsumer + payload
  User->>+API: GET api/message/john
  API->>RabbitMQ: IGreetingCmd + john
  RabbitMQ->>Worker: GreetingCmdConsumer + john
  Worker->>RabbitMQ: GreetingCmdResponse + 'Hola john'
  RabbitMQ->>API: GreetingCmdResponse + 'Hola john'
  API->>-User: HTTP 200 Hola john
```

## Ejemplo

En la carpeta `./example` hay un ejemplo de implementacion utilizando `docker-compose`

```bash
cd example
# DOCKER
docker-compose up -d
# NERDCTL
nerdctl compose up -d
```

**Limpieza**

```bash
# DOCKER
docker-compose stop
docker-compose rm
# NERDCTL
nerdctl compose stop
nerdctl compose rm
```

## DEBUG

Para hacer un debug puede utilizar la instancia de RabbitMQ del docker compose ó crear una instancia dedicada (como usan los mismos puerts debe elegir una de las dos opciones NO ambas)

**Herramientas**

- [Visual Studio 2022 Community](https://visualstudio.microsoft.com/es/vs/community/)
- Rancher Desktop
- Instancia de Rabbit

Para crear una instancia de RabbitMQ inedependiente puede utilizar el siguiente comando

```bash
# DOCKER
docker run -d --hostname myrabbit --name rabbitmq -p 15672:15672 -p 5672:5672 -e RABBITMQ_DEFAULT_USER=desa -e RABBITMQ_DEFAULT_PASS=desarrollo masstransit/rabbitmq:latest
# NERDCRL
nerdctl run -d --hostname myrabbit --name rabbitmq -p 15672:15672 -p 5672:5672 -e RABBITMQ_DEFAULT_USER=desa -e RABBITMQ_DEFAULT_PASS=desarrollo masstransit/rabbitmq:latest
```

**Limpieza**
Para eliminar el contenedor creado

```bash
# DOCKER
docker stop myrabbit
docker rm myrabbit

# NERDCTL
nerdctl stop myrabbit
nerdctl rm myrabbit
```

## Contenedor RabbitMQ

Broker de mensajes para la comunicación entre servicios. La imagen es una customización de la imagen oficial de [RabbitMQ en Docker Hub](https://hub.docker.com/_/rabbitmq/)

## Endpoints

| NOMBRE        | PUERTO | PATH       | DESCRIPCION                      |
| ------------- | ------ | ---------- | -------------------------------- |
| AMQP          | 5672   |            | Puerto TPC para bus de mensajes  |
| MANAGEMENT-UI | 15672  | `/`        | Interface HTML de administración |
| METRICAS      | 15692  | `/metrics` | Métricas en formato *Prometheus* |

### Configuración

Settings basicos utilizados por esta implementación

| ENV                     | DESCRIPCION                     |
| ----------------------- | ------------------------------- |
| `RABBITMQ_DEFAULT_USER` | Usuario default                 |
| `RABBITMQ_DEFAULT_PASS` | Password del usuario de default |

## Contenedor API

Recibe comando vía REST. Los encola en el bus de mensajes y envía la respuesta pertinente.

[![Docker Repository on Quay](https://quay.io/repository/unahur.arqsw/messagefun.api/status "Docker Repository on Quay")](https://quay.io/repository/unahur.arqsw/messagefun.api)

### Endpoints API

| NOMBRE     | PUERTO | PATH             | DESCRIPCION                           |
| ---------- | ------ | ---------------- | ------------------------------------- |
| API        | 8080   | `/api`           | Apis REST                             |
| SWAGGER-UI | 8080   | `/swagger`       | Interface HTML de prueba (Swagger-UI) |
| METRICAS   | 8080   | `/metrics`       | Métricas en formato *Prometheus*      |
| HEALTH     | 8080   | `/healthz/live`  | Sonda de servicio VIVO                |
| READY      | 8080   | `/healthz/ready` | Sonda de servicio LISTO               |

### Configuración API

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

### Endpoints Worker

| NOMBRE   | PUERTO | PATH             | DESCRIPCION                      |
| -------- | ------ | ---------------- | -------------------------------- |
| METRICAS | 9090   | `/metrics`       | Métricas en formato *Prometheus* |
| HEALTH   | 9090   | `/healthz/live`  | Sonda de servicio VIVO           |
| READY    | 9090   | `/healthz/ready` | Sonda de servicio LISTO          |

### Configuración Worker

IDEM API
