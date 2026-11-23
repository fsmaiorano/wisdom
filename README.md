# Wisdom

Wisdom is a video-based learning platform built as a set of microservices. Instructors create courses and upload lesson videos, students buy courses and watch lessons, and progress is tracked per user.

## Architecture

```
                        ┌──────────────┐
   Angular UI  ───────► │     BFF      │ ◄── JWT (Keycloak)
 (wisdom-ui :6001)      │  (.NET :6100)│
                        └──────┬───────┘
        ┌──────────┬──────────┼───────────┬────────────┐
        ▼          ▼          ▼           ▼            ▼
     User API  Course API  Media API  Order API  Learning API   (.NET, one Postgres DB each)
                              │
                              │ RabbitMQ (outbox events)
                              ▼
                    Media Worker (.NET) ──► SeaweedFS (S3)  ◄── Streaming (.NET)
```

### .NET services (`dotnet/`)

| Project | Description |
|---|---|
| `Wisdom.Bff` | Single entry point. Validates the JWT, forwards internal auth headers and aggregates data from the backends. |
| `Wisdom.User` | Users and Keycloak integration. |
| `Wisdom.Course` | Courses, lessons and tags. |
| `Wisdom.Media` | Media metadata, upload, outbox and idempotency. Publishes events to RabbitMQ. |
| `Wisdom.Order` | Course purchases. |
| `Wisdom.Learning` | Lesson progress tracking. |
| `Wisdom.MediaWorker` | Background worker. Consumes `MediaReadyIntegrationEvent` from RabbitMQ, transcodes videos with ffmpeg (480p/720p/1080p + thumbnail), uploads them to SeaweedFS and marks the media as `Ready`. A reconciler retries media stuck in `Uploaded`/`Failed`. |
| `Wisdom.Streaming` | HTTP service that streams videos from SeaweedFS with range request support (`GET/HEAD /stream/{mediaId}?quality=480p\|720p\|1080p`). |
| `Wisdom.BuildingBlocks` | Shared code (database initialisation, common services). |
| `Wisdom.Tests.Common` | Shared test helpers. |
| `Wisdom.Simulator` | Interactive console tool that exercises the whole system through the BFF. |

Each domain service follows the same layered layout (`Wisdom.MediaWorker` and `Wisdom.Streaming` are lighter: they have no `Domain` and no `Infrastructure.Migration`, because the `Media` table and its migrations belong to `Wisdom.Media`):

```
Wisdom.<Service>/
├── Wisdom.<Service>.Api                       # ASP.NET Core Web API (startup project)
├── Wisdom.<Service>.Application               # Use cases
├── Wisdom.<Service>.Domain                    # Entities and domain logic
├── Wisdom.<Service>.Infrastructure            # EF Core DbContext, repositories
├── Wisdom.<Service>.Infrastructure.Migration  # EF Core migrations
├── Wisdom.<Service>.UnitTests
├── Wisdom.<Service>.IntegrationTests
├── Wisdom.<Service>.sln
└── migrate.sh                                 # Migration helper
```

### Frontend (`ui/wisdom-ui`)

Angular single-page application served by nginx in Docker.

### Infrastructure (`infra/`)

- `infra/compose/docker-compose.yml`: the whole stack.
- `infra/keycloak/`: realm export (`wisdom`) and entrypoint scripts.

## Tech stack

| Area | Technologies |
|---|---|
| Backend | .NET 10, ASP.NET Core, EF Core 10, Npgsql, Swashbuckle (Swagger), JWT Bearer authentication |
| Logging and tracing | Serilog (Console, Seq and OpenTelemetry sinks), OpenTelemetry (ASP.NET Core, HTTP) |
| Messaging client | RabbitMQ.Client (via `Wisdom.BuildingBlocks` event bus) |
| Object storage client | MinIO .NET SDK 6, used only as an S3-compatible client against SeaweedFS (`Wisdom.Media`, `Wisdom.MediaWorker`, `Wisdom.Streaming`) |
| Video processing | ffmpeg / ffprobe |
| Frontend | Angular 21, Angular Material/CDK, RxJS, TypeScript 5.9, Vitest |
| Database | PostgreSQL 17 (one database per service) |
| Identity | Keycloak 26.1 |
| Messaging | RabbitMQ 3 (management) |
| Object storage | SeaweedFS (S3 API on `:9000`, master UI on `:9333`, filer UI on `:8888`); replaces MinIO |
| Observability | Seq |
| Containers | Docker Compose; the UI is served by nginx |
| Tests | xUnit, coverlet, Bogus |
| Simulator | Spectre.Console, Bogus |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- EF Core CLI: `dotnet tool install --global dotnet-ef`
- [Docker](https://www.docker.com/) with Docker Compose
- [ffmpeg](https://ffmpeg.org/) (only to run `Wisdom.MediaWorker` outside Docker; the Docker image already includes it)
- Node.js and npm 11 (only to run the UI outside Docker)

## Getting started

### Option 1: run everything with Docker

```bash
docker compose -f infra/compose/docker-compose.yml up -d --build
```

Services use the `appsettings.Docker.json` configuration. Databases are migrated automatically when each API starts.

### Option 2: infrastructure in Docker, services locally

1. Start only the infrastructure:

   ```bash
   docker compose -f infra/compose/docker-compose.yml up -d \
     keycloakdb userdb coursedb mediadb orderdb learningdb keycloak seaweedfs rabbitmq seq
   ```

2. Run the APIs you need (each in its own terminal):

   ```bash
   cd dotnet/Wisdom.User/Wisdom.User.Api && dotnet run
   cd dotnet/Wisdom.Course/Wisdom.Course.Api && dotnet run
   cd dotnet/Wisdom.Media/Wisdom.Media.Api && dotnet run
   cd dotnet/Wisdom.Order/Wisdom.Order.Api && dotnet run
   cd dotnet/Wisdom.Learning/Wisdom.Learning.Api && dotnet run
   cd dotnet/Wisdom.Bff/Wisdom.Bff && dotnet run
   cd dotnet/Wisdom.MediaWorker/Wisdom.MediaWorker.Worker && dotnet run
   cd dotnet/Wisdom.Streaming/Wisdom.Streaming.Api && dotnet run
   ```

3. Run the UI:

   ```bash
   cd ui/wisdom-ui
   npm install
   npm start
   ```

To build everything in the .NET solution: `dotnet build dotnet/Wisdom.sln`.

## Ports

Defined in [`docs/port-mapping.md`](docs/port-mapping.md).

| Service | Local env | Docker env | Inside Docker |
|---|---|---|---|
| Bff | 5001 / 5051 | 6100 / 6150 | 8080 |
| User API | 5006 / 5066 | 6006 / 6066 | 8080 |
| Course API | 5007 / 5077 | 6007 / 6077 | 8080 |
| Media API | 5008 / 5088 | 6008 / 6088 | 8080 |
| Order API | 5009 / 5099 | 6009 / 6099 | 8080 |
| Learning API | 5010 / 5100 | 6010 / 6100 | 8080 |

| Other | Local env | Docker env | Inside Docker |
|---|---|---|---|
| KeycloakDb | 5400 | 5400 | 5432 |
| UserDb | 5401 | 5401 | 5432 |
| CourseDb | 5402 | 5402 | 5432 |
| LearningDb | 5403 | 5403 | 5432 |
| MediaDb | 5404 | 5404 | 5432 |
| OrderDb | 5405 | 5405 | 5432 |
| Seq (UI/API) | 5500 | 5500 | 80 |
| Keycloak | 8180 | 8180 | 8080 |
| RabbitMQ (AMQP / management) | 5672 / 15672 | 5672 / 15672 | same |
| SeaweedFS (S3 / master / filer) | 9000 / 9333 / 8888 | 9000 / 9333 / 8888 | same |
| Streaming | 5011 | 6011 | 8080 |
| Media Worker (health only) | 5012 | 6012 | 8080 |
| UI | `ng serve` (4200) | 6001 | 80 |

## Configuration

Each API has `appsettings.json`, `appsettings.Development.json` and `appsettings.Docker.json`. The main settings are:

- `ConnectionStrings:DefaultConnection`: PostgreSQL, for example `Host=localhost;Port=5401;Database=UserDb;Username=postgres;Password=postgres`.
- `ConnectionStrings:RabbitMq`: for example `amqp://admin:admin@localhost:5672/`.
- `Keycloak`: `BaseUrl`, `Realm` (`wisdom`), `ClientId` (`wisdom-app`), admin credentials.

> The credentials above (`postgres/postgres`, `admin/admin`) are for **local development only**. Never use them in production.

Service-specific settings:

- **Media Worker** (`Wisdom.MediaWorker.Worker/appsettings.json`): `EventBus:PrefetchCount` (1, so one video is transcoded at a time), `Reconciler:IntervalSeconds` (60), `Reconciler:StaleAfterSeconds` (300), `Storage:*` (SeaweedFS endpoint, credentials, `BucketName`). The queue is `wisdom.MediaReadyIntegrationEvent.MediaWorkerHandler`, with its dead-letter exchange/queue (7-day TTL).
- **Streaming** (`Wisdom.Streaming.Api/appsettings.json`): `InternalAuth:ApiKey` (the BFF sends it in the `X-Internal-Api-Key` header) and `Streaming:DisableAuth` (`true` only in `appsettings.Development.json`).

- **Media API** (`Wisdom.Media.Api/appsettings*.json`): `Storage:*` (`Endpoint`, `AccessKey`, `SecretKey`, `UseSSL`) pointing at the SeaweedFS S3 API (`localhost:9000`, or `seaweedfs:9000` in Docker). Local credentials are `admin` / `admin123`.

Useful local URLs: Keycloak `http://localhost:8180`, RabbitMQ UI `http://localhost:15672`, Seq `http://localhost:5500`, SeaweedFS master UI `http://localhost:9333`, SeaweedFS filer UI `http://localhost:8888` (objects of the `wisdom-media` bucket are under `/buckets/wisdom-media/`).

## Database migrations

Every service has its own database and its own migration project, `Wisdom.<Service>.Infrastructure.Migration`. EF Core commands use the migration project as `--project` and the Api project as the startup project, so they must run from inside `Wisdom.<Service>.Api`.

APIs apply pending migrations automatically on startup (`InitialiseDatabaseAsync<TContext>` in `Wisdom.BuildingBlocks`), creating the database if needed. The commands below are for creating new migrations or applying them manually.

### Using the helper script

Each service folder (`User`, `Course`, `Media`, `Order`, `Learning`) has a `migrate.sh`. It can be run from any directory:

```bash
cd dotnet/Wisdom.User

./migrate.sh add <MigrationName>    # create a new migration
./migrate.sh update                 # apply all pending migrations
./migrate.sh update <Target>        # update (or roll back) to a specific migration
./migrate.sh list                   # list migrations
./migrate.sh remove                 # remove the last unapplied migration
```

### Using `dotnet ef` directly

```bash
cd dotnet/Wisdom.User/Wisdom.User.Api

dotnet ef migrations add <MigrationName> \
  --project ../Wisdom.User.Infrastructure.Migration \
  --output-dir Migrations

dotnet ef database update --project ../Wisdom.User.Infrastructure.Migration
dotnet ef migrations list --project ../Wisdom.User.Infrastructure.Migration
dotnet ef migrations remove --project ../Wisdom.User.Infrastructure.Migration
```

### Per service

#### Wisdom.User.Infrastructure.Migration

```bash
cd dotnet/Wisdom.User && ./migrate.sh add <MigrationName>
# Database: UserDb (localhost:5401)
```

#### Wisdom.Course.Infrastructure.Migration

```bash
cd dotnet/Wisdom.Course && ./migrate.sh add <MigrationName>
# Database: CourseDb (localhost:5402)
```

#### Wisdom.Learning.Infrastructure.Migration

```bash
cd dotnet/Wisdom.Learning && ./migrate.sh add <MigrationName>
# Database: LearningDb (localhost:5403)
```

#### Wisdom.Media.Infrastructure.Migration

```bash
cd dotnet/Wisdom.Media && ./migrate.sh add <MigrationName>
# Database: MediaDb (localhost:5404)
```

#### Wisdom.Order.Infrastructure.Migration

```bash
cd dotnet/Wisdom.Order && ./migrate.sh add <MigrationName>
# Database: OrderDb (localhost:5405)
```

### Generating SQL scripts

`Wisdom.User` also ships `sqlscript.sh`, which generates SQL files into `Wisdom.User.Infrastructure.Migration/Scripts/`:

```bash
cd dotnet/Wisdom.User

./sqlscript.sh complete                       # one idempotent script with all migrations
./sqlscript.sh per-migration                  # one script per migration step
./sqlscript.sh complete --no-idempotent       # without idempotency guards
```

For the other services, use `dotnet ef migrations script --idempotent --project ../Wisdom.<Service>.Infrastructure.Migration --output <file>.sql` from the Api folder.

## Running tests

```bash
# all tests of one service
dotnet test dotnet/Wisdom.User/Wisdom.User.sln

# a single project
dotnet test dotnet/Wisdom.Course/Wisdom.Course.UnitTests
```

`Wisdom.MediaWorker` and `Wisdom.Streaming` follow the same layout and have their own solutions and `UnitTests` / `IntegrationTests` projects (integration tests replace the database, storage, ffmpeg and RabbitMQ with fakes, so no infrastructure is needed):

```bash
dotnet test dotnet/Wisdom.MediaWorker/Wisdom.MediaWorker.sln
dotnet test dotnet/Wisdom.Streaming/Wisdom.Streaming.sln
```

Frontend tests: `cd ui/wisdom-ui && npm test`.

## API documentation and tooling

- **Swagger**: available on each API in the Development environment.
- **Bruno collection**: `docs/bruno/wisdom` (folders: auth, users, courses, lessons, tags, media, orders, progress, aggregations). Open it with [Bruno](https://www.usebruno.com/) and select the `docker` or local environment.
- **Seq**: structured logs at `http://localhost:5500`.

## Simulator

`dotnet/Wisdom.Simulator` is an interactive console app that drives the system through the BFF, creating users, courses, lessons, orders and progress with generated data (Bogus).

```bash
cd dotnet/Wisdom.Simulator
dotnet run                                  # asks for the environment
BASE_URL=http://localhost:6100 dotnet run   # skip the prompt (Docker BFF)
```

Environments: Local (`http://localhost:5000`) or Docker (`http://localhost:6100`). Scenarios:

1. Create a course with lessons.
2. Create a course, lessons and buy it.
3. Create a course and lessons; X users buy and watch (loop).
4. Create a course; X users buy and watch with random progression.
5. Create a course; X users buy and watch, and show the BFF aggregation.
6. **Demo setup**: the seeded teacher (`teacher@wisdom.dev` / `demoteacher`) creates and publishes a course with six varied lessons, each with a video from `mocks/`, and the seeded student (`customer@wisdom.dev` / `democustomer`) buys it. No progress is recorded, so signing in as the student shows an enrolled course ready to play. The scenario can wait for the Media Worker to finish transcoding (about 2 minutes per video); lessons become playable as they reach `Ready`.

Defaults (`Config/SimulatorSettings.cs`): 5 lessons per course, 3 customers, 1000 ms watch delay. The full stack must be running before starting the simulator.

## Mocks

The `mocks/` folder holds sample media (a test `.mp4` video) used to try the upload, processing (worker) and streaming flow. Upload it through the Media API (Bruno `06-media`) or use it as lesson content, then check that the worker processes it and that the streaming service plays it.

## Troubleshooting

- **`dotnet ef` not found**: install it with `dotnet tool install --global dotnet-ef`.
- **API cannot connect to the database**: make sure the matching Postgres container is healthy (`docker compose ps`) and that the port matches the table above.
- **Keycloak slow to start**: it can take about 90 seconds on first boot; wait for the health check to pass.
- **Reset all data**: `docker compose -f infra/compose/docker-compose.yml down -v` removes the volumes.
