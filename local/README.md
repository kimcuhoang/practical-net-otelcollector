# .NET with OpenTelemetry Collector

## Overview

![OpenTelemetry Collector deployed as gateway](https://assets.cdn.prod.twilio.com/original_images/P2WQkAXtj_hI3uS3vRIqHk3y3C_zSPW4m9r6pJeh8vHQLcQstXzEcbQcjzN9uiHmyspNk1f4aEYO6_)

_I borrowed the image from [Automatic Instrumentation of Containerized .NET Applications With OpenTelemetry](https://www.twilio.com/blog/automatic-instrumentation-of-containerized-dotnet-applications-with-opentelemetry)_

## Starting Locally

```bash

docker compose -f ./local/infra/docker-compose.observability.yaml up -d

```

```bash

tye run ./local/tye/tye.yaml --dashboard

```

## Observability

- Making the 1st request `http://localhost:6001`
- Access `grafana` at `http://localhost:3000` to explore 4 datasources
  - Loki
  - Jaeger
  - Zipkin
  - Prometheus


