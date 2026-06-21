# RIMAPI — AI Agent Guide

RIMAPI is a RimWorld mod that embeds a REST API server directly into the game, exposing 120+ endpoints on `http://localhost:8765/` so external applications can read and interact with the colony in real-time.

## Tech Stack

- **Language:** C# 9.0 targeting .NET Framework 4.7.2
- **Serialization:** Newtonsoft.Json 13.0 with a custom `SnakeCaseContractResolver`
- **Patching:** Harmony 2.3.3 for runtime method hooks
- **RimWorld versions:** 1.5 and 1.6 (both built from the same source via conditional compilation)
- **Build configs:** `Debug`, `Release-1.5`, `Release-1.6`

## Repository Layout

```
My_test_mod/
├── About/                        # Mod metadata (About.xml, Manifest.xml, preview.png)
├── Source/RIMAPI/                # All C# source
│   ├── RIMAPI_Mod.cs             # Entry point, settings UI
│   ├── RIMAPI_GameComponent.cs   # Server lifecycle, tick processor
│   ├── RIMAPI_Settings.cs        # User-configurable settings
│   └── RimworldRestApi/
│       ├── Core/                 # HTTP server, router, DI, caching, SSE, logging
│       ├── Controllers/          # 25 controllers across 5 domains (thin — parse + respond only)
│       ├── Services/             # Business logic; abstracts RimWorld's internal API
│       ├── Models/               # DTOs — never expose RimWorld internals in responses
│       ├── Hooks/                # Harmony patches that publish SSE events
│       ├── Helpers/              # Reusable utility functions
│       └── Camera/               # UDP camera streaming
├── 1.5/Assemblies/               # Compiled DLL for RimWorld 1.5
├── 1.6/Assemblies/               # Compiled DLL for RimWorld 1.6
├── Libraries/                    # Newtonsoft.Json.dll
├── docs/                         # MkDocs documentation source
│   ├── api/                      # Endpoint reference by domain
│   ├── developer_guide/          # API conventions, extension guide, endpoint guide
│   └── contributors_guide/       # Architecture, contributing, release flow
├── tests/                        # Python integration scripts + Bruno API collection
├── scripts/                      # bump_version.py, doc generation helpers
├── RimApi.sln / RimApi.csproj    # Visual Studio solution
└── CHANGELOG.md
```

## Architecture

### Request Pipeline

```
HTTP Client
  → HttpListener (background thread)
  → Request Queue  (producer-consumer, thread-safe)
  → ApiServer.ProcessTick()  (main Unity thread, ≤10 req/tick)
  → Router  (attribute-based pattern matching)
  → Controller  (parse request, call service, return ApiResult)
  → Service  (all RimWorld API access lives here)
  → JSON response
```

All game-state mutations happen on the main thread automatically. Response latency depends on game frame rate and queue depth.

### Dependency Injection

Custom DI container (`ServiceCollection` + `ServiceProvider`). Singletons: `ApiServer`, `SseService`, `EventRegistry`. Controllers are transient. Constructor injection is used throughout.

### Server-Sent Events (SSE)

`SseService` broadcasts real-time game events (pawn death, incidents, map changes, etc.) to connected clients. Events are published from Harmony hooks in `Hooks/`. Extensions can publish custom events.

### Extension System

Other mods can add endpoints by implementing `IRimApiExtension`. The registry discovers implementations via reflection, isolates their errors, and registers their services and routes automatically.

### Caching

`CachingService` provides in-memory TTL caching with strategies: Absolute, Sliding, Never, GameTick-based. Priority levels: Low, Normal, High, Critical. Controllers call `CacheAwareResponseAsync(...)` for cached endpoints.

## Coding Conventions

### Controllers are thin

Controllers only parse the request and return a response. All logic goes in services. No RimWorld API calls in controllers.

```csharp
// Good
[Get("/api/v1/colonists")]
public async Task GetColonists(HttpListenerContext context)
{
    var colonists = _colonistService.GetAll();
    await context.WriteJsonAsync(ApiResult<List<ColonistDto>>.Ok(colonists));
}
```

### Services abstract RimWorld

Services own all interaction with RimWorld internals. Never let a DTO or controller touch `Verse.*` types directly — map everything to a DTO first.

### Models (DTOs)

- All DTOs live in `Source/RIMAPI/RimworldRestApi/Models/`
- Organized by domain: `Pawns/`, `Game/`, `Map/`, `Items/`, `UI/`, `Camera/`, etc.
- C# PascalCase properties auto-convert to `snake_case` JSON keys via `SnakeCaseContractResolver`
- A few DTOs override this with explicit `[JsonProperty("key")]` (e.g. `WorkPriorityRequestDto`)

### Helpers vs Services

- **Helpers** contain stateless reusable code (pure functions, conversions)
- **Services** orchestrate workflows and hold state/dependencies

## API Conventions

### Response Envelope

Every response is wrapped:

```json
{
  "success": true,
  "data": { ... },
  "errors": [],
  "warnings": [],
  "timestamp": "2026-03-21T23:26:03.876Z"
}
```

Use `ApiResult<T>` (with data) or `ApiResult` (no data). `data` is omitted when null.

### HTTP Status Codes

| Status | Trigger |
|--------|---------|
| 200 | `success: true` |
| 400 | Error message contains "validation" |
| 401 | Error message contains "unauthorized" |
| 404 | Error message contains "not found" |
| 411 | POST without `Content-Length` header |
| 500 | All other errors |

### JSON Naming

All keys use `snake_case`. Examples:

| C# Property | JSON Key |
|-------------|----------|
| `MapId` | `map_id` |
| `IsDrafted` | `is_drafted` |
| `PlantDef` | `plant_def` |
| `X`, `Y`, `Z` | `x`, `y`, `z` |

### POST Requests

All POST requests must include a `Content-Length` header (required by .NET's `HttpListener`). Send `{}` if the body is empty.

### IDs

Pawn, building, zone, and map IDs are **integers** (RimWorld's internal `thingIDNumber`).

```json
{ "pawn_id": 184 }   // correct
{ "pawn_id": "184" } // wrong — deserializes as 0
```

### defNames

RimWorld definition names are **case-sensitive**: `Plant_Potato`, `Electricity`, `Growing`.

### Coordinates

RimWorld uses 3D coords; Y is always `0` for ground level. Rectangular areas use `point_a` + `point_b`.

```json
{ "position": {"x": 120, "y": 0, "z": 130} }
```

### Query Parameters

Query param names are **not** snake_cased — use the exact name from the `RequestParser` call in the controller.

## Adding a New Endpoint

1. **Create a DTO** in the appropriate `Models/` subdirectory.
2. **Create a service interface + implementation** in `Services/<domain>/`.
3. **Register the service** in `ApiServer.CreateDefaultServiceProvider()`.
4. **Create or update a controller** in `Controllers/<domain>/`, inject the service, add a `[Get]`/`[Post]`/etc. method.
5. **Update docs** in `docs/_endpoints_examples/examples.yml` (required for release).

Route attributes are discovered automatically — no manual registration needed.

## Build & Versioning

### Building

Open `Source/RimApi.sln`. Build with `Release-1.5` for RimWorld 1.5 output to `1.5/Assemblies/`, or `Release-1.6` for `1.6/Assemblies/`. Both must be built before releasing.

Conditional compilation: `RIMWORLD_1_5` and `RIMWORLD_1_6` symbols are set per configuration.

### Version Bump

Never edit version strings manually. Run from the repo root:

```bash
python scripts/bump_version.py 1.9.1
```

This updates `About/About.xml`, `About/Manifest.xml`, `Source/RIMAPI/RimApi.csproj`, and doc metadata atomically.

## Testing

### Integration Tests (Python)

Located in `tests/`. Require RIMAPI running in-game.

```bash
python tests/spawn_and_edit.py      # pawn creation & modification
python tests/spawn_battle_test.py   # combat scenarios
python tests/sse_debugger.py        # SSE event stream debug
```

### Bruno API Collection

`tests/bruno_api_collection/` contains pre-configured HTTP requests for manual endpoint testing. The full collection should pass before any release.

### Manual Checklist

Before submitting a PR:
1. Mod loads without errors in RimWorld
2. API server starts on the configured port
3. New endpoint responds with correct data and status codes
4. A few existing endpoints still work (regression check)
5. Error paths return proper 4xx/5xx responses

## Release Process

1. Finalize `CHANGELOG.md` — move `[Unreleased]` to the new version + date
2. Run doc formatting/validation scripts in `scripts/docs/`
3. Run `python scripts/bump_version.py <version>`
4. Build for both 1.5 and 1.6
5. Commit: `git commit -m "Release v1.9.1"` then `git tag -a v1.9.1 -m "Release v1.9.1"`
6. Push commit and tag

## Domain Map

| Domain | Controllers | Key Services |
|--------|-------------|-------------|
| System | GameController, ThingsController, DevToolsController | IGameStateService, IGameDataService, IDevToolsService |
| Colony | BillController, ResearchController, OrderController, TradeController, GameEventsController, BuilderController, ColonistsWorkController | IBillService, IResearchService, ITradeService, IIncidentService, IBuilderService |
| Pawns | PawnController, PawnInfoController, PawnEditController, PawnJobController, PawnSocialController, PawnSpawnController | IColonistService, IPawnInfoService, IPawnEditService, IPawnJobService, IPawnSocialService, IPawnSpawnService |
| World | MapController, GlobalMapController, FactionController | IMapService, IGlobalMapService, IFactionService, IBuildingService |
| Client | UIController, CameraController, ImagesController, OverlayController, LearningController | IUIService, ICameraService, IImageService, IOverlayService, ILearningService |
| AI | LordController | — |

## Key Files

| File | Purpose |
|------|---------|
| [Source/RIMAPI/RIMAPI_Mod.cs](Source/RIMAPI/RIMAPI_Mod.cs) | Mod entry point, Harmony init, settings UI |
| [Source/RIMAPI/RIMAPI_GameComponent.cs](Source/RIMAPI/RIMAPI_GameComponent.cs) | Server lifecycle, request tick processor |
| [Source/RIMAPI/RimworldRestApi/Core/ApiServer.cs](Source/RIMAPI/RimworldRestApi/Core/ApiServer.cs) | HTTP orchestration, DI setup, route registration |
| [Source/RIMAPI/RimworldRestApi/Core/Routing/Router.cs](Source/RIMAPI/RimworldRestApi/Core/Routing/Router.cs) | Attribute-based routing |
| [Source/RIMAPI/RimworldRestApi/Core/SSE/SseService.cs](Source/RIMAPI/RimworldRestApi/Core/SSE/SseService.cs) | Real-time event broadcasting |
| [Source/RIMAPI/RimworldRestApi/Core/Caching/CachingService.cs](Source/RIMAPI/RimworldRestApi/Core/Caching/CachingService.cs) | TTL caching |
| [docs/api/index.md](docs/api/index.md) | Master API endpoint reference |
| [docs/developer_guide/api_conventions.md](docs/developer_guide/api_conventions.md) | Request/response standards |
| [docs/contributors_guide/architecture.md](docs/contributors_guide/architecture.md) | Architecture diagrams and lifecycle flows |
