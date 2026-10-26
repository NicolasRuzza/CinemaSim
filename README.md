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

## Prerequisites

- .NET 10 SDK.
- Docker, for the local infrastructure above and for the Redis contract tests.

## Tooling

- ASP.NET Core Web API with Vertical Slice Architecture: each use case lives in `src/Cinema.Api/Features/<Area>/<UseCase>/` with its command, handler, result and ports. A port that more than one host needs, such as the seat lock, lives in `src/Cinema.Infrastructure` with its adapter.
- [Mediator](https://github.com/martinothamar/Mediator) (source generator, MIT) for commands and queries. The generator is installed only in the host project, `Cinema.Api`.
- Reqnroll + xUnit for BDD scenarios, and AwesomeAssertions (Apache-2.0) for assertions.
- [Moq](https://github.com/devlooped/moq) (BSD-3-Clause, 4.20.2 or newer) provides the test doubles in the unit tests.
- `FakeTimeProvider` (Microsoft.Extensions.TimeProvider.Testing, MIT) moves time forward instantly in scenarios, so a 5-minute hold expires without waiting.
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) (MIT) backs the seat lock. `RedisSeatLockStore` runs a Lua script so checking and taking a seat is one atomic step.
- [Testcontainers](https://dotnet.testcontainers.org/) (MIT) starts a throwaway Redis for the contract tests.
- Use-case settings follow the .NET Options pattern, for example `HoldSeatOptions.HoldDuration` (5 minutes by default).

## Run the API

The hold endpoint only needs Redis. From the repository root:

```sh
docker compose up -d redis
dotnet run --project src/Cinema.Api
```

The API listens on `http://localhost:5169` (Development profile). Try the requests in [`src/Cinema.Api/Cinema.Api.http`](src/Cinema.Api/Cinema.Api.http), or with curl:

```sh
curl -i -X POST http://localhost:5169/api/v1/sessions/S1/seats/hold \
  -H "Content-Type: application/json" \
  -d '{"seatId":"A1","customerId":"Bob"}'
```

| Status | When |
|---|---|
| `200` | The seat is held for the customer. The same customer repeating the request also gets `200`, and the five minutes are not extended. |
| `409` | Another customer holds the seat. |
| `404` | The session or the seat does not exist. |
| `400` | `seatId` or `customerId` is missing or blank. |

Errors are returned as `application/problem+json`. The Phase 1 schedule is mocked: sessions `S1` to `S5`, seats `A1` to `E8`. In Development, the OpenAPI document is at `/openapi/v1.json`.

| Setting | Environment variable | Default |
|---|---|---|
| `ConnectionStrings:Redis` | `ConnectionStrings__Redis` | `localhost:6379` in `appsettings.Development.json`. Required elsewhere: without it the API refuses to start. |
| `HoldSeat:HoldDuration` | `HoldSeat__HoldDuration` | `00:05:00`. Must be greater than zero; checked when the API starts. |

## Run the tests

```sh
dotnet build CinemaSim.slnx
dotnet test tests/Cinema.UnitTests
dotnet test tests/Cinema.BddTests
dotnet test tests/Cinema.IntegrationTests
```

- `Cinema.UnitTests`: the hold seat handler in isolation, with its ports replaced by Moq doubles.
- `Cinema.BddTests`: the business rules as Gherkin scenarios, run in memory.
- `Cinema.IntegrationTests`: real pieces working together, described below.

`Cinema.IntegrationTests` holds the seat lock contract tests: one suite of rules that every `ISeatLockStore` must honor. It runs against the in-memory fake and against a real Redis, so the fake used by the BDD scenarios cannot drift from the adapter. The Redis tests start a container through Testcontainers, so `dotnet test` must be able to reach Docker (for example, run it from WSL, where Docker lives). Skip them with `--filter "Requires!=Docker"`. The project also holds the HTTP endpoint tests (the real API in memory, with the lock swapped for the fake) and smoke tests that run the whole stack against a real Redis. All Docker tests share one container and run one after another, because some of them depend on real time.

Scenarios are in `tests/Cinema.BddTests/Features`. Reqnroll generates the `*.feature.cs` files on every build; they are git-ignored. A scenario without step definitions fails, so a scenario that is specified but not implemented yet carries the `@ignore` tag: it shows as skipped and keeps the build green. Remove the tag in the pull request that implements the scenario. The in-memory seat lock used by the scenarios is a test double: real atomicity and expiration are verified against Redis in the integration tests.

## Project direction

1. Seat holds with an atomic Redis operation and a five-minute TTL. Implemented: `POST /api/v1/sessions/{sessionId}/seats/hold`.
2. Publish purchase confirmation events to Redpanda and process them asynchronously into PostgreSQL.
3. Add a resilient background worker to synchronize movie data from OMDb.
4. Plan the Angular web client as a future product step.

Kubernetes and Helm deployment files are also planned for a later stage.
