# API Conventions

To ensure a consistent and standard RESTful experience, RIMAPI enforces the following rules globally across all endpoints.

## Response Envelope

All responses are wrapped in a standardized envelope. The structure depends on whether the endpoint returns data:

**With data** (`ApiResult<T>`):
```json
{
  "success": true,
  "data": { ... },
  "errors": [],
  "warnings": [],
  "timestamp": "2026-03-21T23:26:03.876Z"
}
```

**Without data** (`ApiResult`):
```json
{
  "success": true,
  "errors": [],
  "warnings": [],
  "timestamp": "2026-03-21T23:26:03.876Z"
}
```

**Partial success** (some operations succeeded, some failed):
```json
{
  "success": true,
  "errors": [],
  "warnings": ["Pawn 42 could not accept job"],
  "timestamp": "2026-03-21T23:26:03.876Z"
}
```

Always check the `success` field. The `data` field is only present on endpoints that return data and is omitted (not `null`) when `NullValueHandling.Ignore` applies.

### HTTP Status Codes

Status codes are derived from error message content:

| Status | Condition |
|--------|-----------|
| 200 | `success: true` (with or without warnings) |
| 400 | Error message contains "validation" |
| 401 | Error message contains "unauthorized" |
| 404 | Error message contains "not found" |
| 411 | POST request without `Content-Length` header (see below) |
| 500 | All other errors |

## Content-Length Header Requirement

All `POST` endpoints require a `Content-Length` header. The underlying .NET `HttpListener` rejects POST requests that omit this header with an HTTP 411 "Length Required" response.

Even if the request body is empty or the endpoint only uses query parameters, you must send a body (an empty JSON object `{}` is sufficient).

Most HTTP clients handle this automatically:

| Client | Behavior |
|--------|----------|
| Python `httpx` | Sends Content-Length automatically when `json=` is used |
| Python `requests` | Sends Content-Length automatically when `json=` is used |
| `curl` | Requires `-d '{}'` or `--data '{}'` to include a body |
| JavaScript `fetch` | Sends Content-Length automatically with `body: JSON.stringify({})` |

**Example with curl:**
```bash
# This fails with 411 Length Required:
curl -X POST http://localhost:8765/api/v1/research/target?name=Electricity

# This works:
curl -X POST -H "Content-Type: application/json" -d '{}' \
  http://localhost:8765/api/v1/research/target?name=Electricity
```

## JSON Naming Standard (`snake_case`)

All JSON request bodies and response payloads use **`snake_case`**. This is enforced by `SnakeCaseContractResolver` which converts C# `PascalCase` property names at the API boundary.

**How conversion works:**

| C# Property | JSON Key |
|-------------|----------|
| `MapId` | `map_id` |
| `PlantDef` | `plant_def` |
| `PointA` | `point_a` |
| `IsDrafted` | `is_drafted` |
| `X`, `Y`, `Z` | `x`, `y`, `z` (single-letter properties stay lowercase) |

**Example request:**

```json
{
  "map_id": 0,
  "plant_def": "Plant_Potato",
  "point_a": {"x": 115, "y": 0, "z": 130},
  "point_b": {"x": 120, "y": 0, "z": 135}
}
```

### `[JsonProperty]` Override

A small number of DTOs use explicit `[JsonProperty("name")]` attributes that **override** the snake_case resolver. In these cases, use the exact key specified in the attribute, not the snake_case conversion.

Currently, only `WorkPriorityRequestDto` uses this pattern:

```json
// POST /api/v1/colonist/work-priority
// Uses [JsonProperty("id")], [JsonProperty("work")], [JsonProperty("priority")]
{"id": 184, "work": "Growing", "priority": 1}
```

All other DTOs use the standard snake_case convention.

### `MissingMemberHandling.Ignore`

The deserializer ignores unknown JSON fields without raising errors. You can include extra fields in your request body and they will be silently discarded. This allows forward compatibility — clients can send fields that older RIMAPI versions don't recognize.

### `NullValueHandling.Ignore`

Null values are omitted from both serialization (responses) and deserialization (requests):

- **Responses**: Fields with `null` values are not included in the JSON output.
- **Requests**: If a DTO property has no matching JSON key and no default initializer, it remains at its C# default (`null` for reference types, `0` for integers, `false` for booleans).

This means **optional fields can simply be omitted** from the request body rather than set to `null`.

## Query Parameters

Some endpoints accept parameters via the URL query string instead of (or in addition to) the request body. Query parameters are **not** affected by the snake_case resolver — use the exact parameter name.

```bash
# Query parameters use their original names
GET /api/v1/colonist?id=184
GET /api/v1/map/weather?map_id=0
POST /api/v1/research/target?name=Electricity
POST /api/v1/map/building/power?buildingId=42&powerOn=true
```

Query parameter names are defined by the `RequestParser` methods in the controller. Check the endpoint documentation or controller source for the exact parameter names.

## ID Types

Pawn, building, zone, and map identifiers are **integers** in the RIMAPI system (corresponding to RimWorld's internal `thingIDNumber`).

```json
// Correct: integer ID
{"pawn_id": 184}

// Incorrect: string ID (will deserialize as 0)
{"pawn_id": "184"}
```

To discover valid IDs:

- **Pawn IDs**: `GET /api/v1/colonists` returns each colonist's `id` field
- **Building IDs**: `GET /api/v1/map/buildings?map_id=0` returns each building's `thing_id` field
- **Zone IDs**: `GET /api/v1/map/zones?map_id=0` returns each zone's `id` field
- **Map IDs**: `GET /api/v1/maps` returns each map's `id` field (usually `0` for the player's home map)

## RimWorld Definition Names

Many endpoints accept RimWorld `defName` strings (research projects, plants, jobs, work types, etc.). These are case-sensitive and must match RimWorld's internal naming.

```bash
# Correct defNames
Plant_Potato        # not "PlantPotato" or "potato"
Electricity         # research project defName
Growing             # work type defName
Goto                # job defName
```

To discover valid definition names:

- **Research projects**: `GET /api/v1/research/tree` lists all research `defName` values
- **Work types**: `GET /api/v1/colonists/work-list` lists all work type names
- **Job definitions**: `GET /api/v1/def/all` includes all registered `JobDef` entries
- **Plant definitions**: Check RimWorld's XML data or use trial and error with the growing zone endpoint (invalid plant defs return a descriptive error)

## Position Coordinates

RimWorld uses a 3D coordinate system, but the map is a 2D grid. The `Y` axis is always `0` for ground-level positions.

```json
{
  "position": {"x": 120, "y": 0, "z": 130}
}
```

For rectangular areas, use `point_a` and `point_b` to define opposite corners:

```json
{
  "point_a": {"x": 115, "y": 0, "z": 130},
  "point_b": {"x": 120, "y": 0, "z": 135}
}
```

The server normalizes the rectangle regardless of which corner is `point_a` vs `point_b`.

## Thread Safety

RIMAPI uses a producer-consumer queue to safely bridge the HTTP server thread and Unity's main game thread. HTTP requests arrive on a background thread, are enqueued, and processed on Unity's main thread during `Update()` (up to 10 requests per frame). This means:

- All game state mutations happen on the correct thread automatically
- Rapid consecutive requests are queued, not dropped
- Response times depend on game frame rate and queue depth
- The game must be running (not frozen on a dialog) for requests to be processed
