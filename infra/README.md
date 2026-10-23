# Local infrastructure

This folder contains service-specific configuration for the development stack. The root `docker-compose.yml` defines the containers, ports, volumes, and network.

## Directories

- `postgres/`: optional database initialization scripts. Application schema and migrations belong in the C# persistence project.
- `redis/`: optional Redis Stack configuration when Compose settings are not enough.
- `redpanda/`: optional broker configuration and development scripts.
- `observability/loki/`: Loki configuration and local log storage settings.
- `observability/alloy/`: Grafana Alloy configuration. Alloy discovers Docker containers through the Docker socket and forwards their logs to Loki.
- `observability/grafana/`: Grafana datasource and dashboard provisioning.

The PostgreSQL, Redis, and Redpanda directories may remain empty while their settings fit in Compose.

## Run and verify

Run these commands from the repository root:

```sh
docker compose up -d
docker compose ps
curl -fsS http://localhost:3100/ready
```

The readiness request should return `ready`. Loki's container logs show that Loki started successfully; Due to problems, Loki's healthcheck was removed, and Alloy and Grafana now wait for the Loki container to start. Alloy retries log delivery while Loki becomes ready.

Useful local endpoints:

- Grafana: `http://localhost:3000` (`admin` / `admin_dev_password`)
- Loki readiness: `http://localhost:3100/ready`
- Alloy UI: `http://localhost:12345`
- Redis Insight: `http://localhost:8001`

To inspect startup or collection issues, use `docker compose logs loki alloy grafana`. Alloy reads Docker logs through `/var/run/docker.sock`; treat that socket as a privileged development dependency.
