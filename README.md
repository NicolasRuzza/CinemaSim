# Cinema Self-Service Backend

A learning project for a cinema self-service backend, built around seat reservation concurrency, asynchronous purchase processing, and observability.

## Repository layout

- `src/`: application projects organized by Vertical Slice Architecture. Each feature groups its request, handler, and feature-specific behavior.
- `tests/`: automated test projects for unit, behavior-focused, and integration tests. k6 scenarios are planned for a later stage.
- `infra/`: service configuration for local development.
- `docker-compose.yml`: local PostgreSQL, Redis Stack, Redpanda, Loki, Alloy, and Grafana services.

## Local infrastructure

From the repository root, start the dependencies with:

```sh
docker compose up -d
docker compose ps
```

Service ports and the Loki readiness check are documented in [`infra/README.md`](infra/README.md). In this development setup, applications started directly from Visual Studio do not produce Docker container logs for Alloy to collect; containerized applications do.

## Project direction

1. Implement seat holds with an atomic Redis operation and a five-minute TTL.
2. Publish purchase confirmation events to Redpanda and process them asynchronously into PostgreSQL.
3. Add a resilient background worker to synchronize movie data from OMDb.
4. Plan the Angular web client as a future product step.

Kubernetes and Helm deployment files are also planned for a later stage.
