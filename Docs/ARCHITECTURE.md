# RIMAPI Architecture Guide

## System Overview

RIMAPI is built as a RimWorld mod that hosts an embedded HTTP server providing REST API access to game data. The architecture is designed for performance, extensibility, and real-time updates.

```
┌─────────────────┐    HTTP/WebSocket     ┌──────────────────┐
│   HTTP Clients  │ ←──────────────────→  │   RIMAPI Server  │
│   (Browsers,    │                       │   (Embedded)     │
│    Apps, etc.)  │                       │                  │
└─────────────────┘                       └──────────────────┘
                                                 │
                                                 │ RimWorld API
                                                 ▼
                                         ┌──────────────────┐
                                         │   RimWorld Game  │
                                         │     Engine       │
                                         └──────────────────┘
```

## Core Components

### 1. Game Component (`RIMAPI_GameComponent`)
- **Purpose**: RimWorld integration point
- **Responsibilities**:
  - Initialize and manage API server lifecycle
  - Process requests during game ticks and GUI events
  - Handle mod settings integration

```csharp
public class RIMAPI_GameComponent : GameComponent
{
    public override void FinalizeInit()
    {
        // Server initialization
        _apiServer = new ApiServer(settings.Port, _gameDataService);
        _apiServer.Start();
    }

    public override void GameComponentTick()
    {
        // Process queued requests every tick
        _apiServer.ProcessQueuedRequests();
    }
}
```

### 2. API Server (`ApiServer`)
- **Purpose**: HTTP server management
- **Responsibilities**:
  - Start/stop HTTP listener
  - Route incoming requests
  - Manage request queue
  - Coordinate WebSocket connections

```csharp
public class ApiServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly Router _router;
    private readonly WebSocketManager _webSocketManager;
    private readonly Queue<HttpListenerContext> _requestQueue;
}
```

### 3. Router (`Router`)
- **Purpose**: Request routing and URL parsing
- **Features**:
  - Pattern-based route matching
  - Path parameter extraction
  - Async request handling

```csharp
// Route registration
_router.AddRoute("GET", "/api/v1/colonists/{id}", 
    context => new ColonistsController().GetColonist(context));

// Route matching extracts {id} parameter
```

### 4. Controllers
- **Purpose**: Handle specific API endpoints
- **Base Features** (via `BaseController`):
  - ETag generation and validation
  - Field filtering
  - Consistent response formatting

```csharp
public class GameController : BaseController
{
    public async Task GetGameState(HttpListenerContext context)
    {
        var gameState = _gameDataService.GetGameState();
        await HandleETagCaching(context, gameState, data => GenerateHash(...));
    }
}
```

### 5. Services (`IGameDataService`)
- **Purpose**: Data access and business logic
- **Responsibilities**:
  - Access RimWorld game data
  - Cache management
  - Data transformation to DTOs

```csharp
public interface IGameDataService
{
    GameStateDto GetGameState();
    List<ColonistDto> GetColonists();
    void RefreshCache();
}
```

### 6. WebSocket Manager (`WebSocketManager`)
- **Purpose**: Real-time event broadcasting
- **Features**:
  - WebSocket connection management
  - Event broadcasting to connected clients
  - Automatic dead connection cleanup

## Data Flow

### HTTP Request Processing
```
1. HTTP Request → HttpListener
2. Request Queued → Request Queue  
3. Tick Processing → Router
4. Route Matching → Controller
5. Data Access → GameDataService
6. Response Building → ResponseBuilder
7. HTTP Response → Client
```

### WebSocket Event Flow
```
1. Game Event → GameDataService.RefreshCache()
2. Cache Update → WebSocketManager.BroadcastGameUpdate()
3. Message Serialization → JSON
4. Broadcast → All Connected Clients
```

## Caching Strategy

### ETag Generation
ETags are generated from data fingerprints to enable conditional requests:

```csharp
// ETag based on data content
await HandleETagCaching(context, data, data => 
    GenerateHash(data.GameTick, data.ColonyWealth, data.ColonistCount));
```

### Cache Refresh
- **Tick-based**: Every `refreshIntervalTicks` (default: 300)
- **On-demand**: When data is requested and cache is stale
- **Event-driven**: When significant game events occur

## Performance Considerations

### Request Processing
- **Dual Processing**: Both `GameComponentTick` and `GameComponentOnGUI`
- **Queue Management**: Process up to 10 requests per tick
- **Async Operations**: Non-blocking I/O operations

### Memory Management
- **DTO Pattern**: Lightweight data transfer objects
- **Connection Pooling**: WebSocket connection management
- **Cache Invalidation**: Tick-based cache refresh

## Extension Points

### 1. Adding New Endpoints
- Create new controller
- Register route in `ApiServer.RegisterRoutes()`
- Implement data access in `IGameDataService`

### 2. Adding New Data Types
- Create DTO classes in `Models/` namespace
- Extend `IGameDataService` interface
- Implement in `GameDataService`

### 3. Adding Real-time Events
- Extend `WebSocketManager.BroadcastGameUpdate()`
- Add new event types
- Subscribe to RimWorld events
