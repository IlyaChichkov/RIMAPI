# RIMAPI - REST API Documentation

## Overview
RIMAPI provides a RESTful API for accessing RimWorld game data in real-time. The API supports HTTP/HTTPS requests with JSON responses, ETag caching, field filtering, and WebSocket for real-time updates.

## Base URL

```
http://localhost:8765/api/v1/
```

## Authentication
Currently no authentication required. All endpoints are accessible locally.

## Common Headers

### Request Headers
- `If-None-Match: "etag-value"` - For conditional requests
- `Accept: application/json` - Response format

### Response Headers  
- `ETag: "abc123"` - Content hash for caching
- `Cache-Control: no-cache` - Client-side caching directive

## Common Parameters

### Fields Filtering
Use `fields` parameter to request specific fields only:
```
GET /api/v1/colonists?fields=name,health,mood
```

### ETag Caching
```csharp
// First request
var response = await client.GetAsync("/api/v1/game/state");
var etag = response.Headers.ETag.Tag;

// Subsequent request with ETag
var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/game/state");
request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag));
var response = await client.SendAsync(request);

if (response.StatusCode == HttpStatusCode.NotModified)
{
    // Use cached data
}
```

## Endpoints

### Version Information
**GET /api/v1/version**

Get API and mod version information.

**Response:**
```json
{
  "version": "1.0.0",
  "rimWorldVersion": "1.4.0",
  "modVersion": "1.0.0",
  "apiVersion": "v1"
}
```

**C# Example:**
```csharp
public async Task<VersionDto> GetVersionAsync()
{
    using var client = new HttpClient();
    var response = await client.GetAsync("http://localhost:8765/api/v1/version");
    response.EnsureSuccessStatusCode();
    
    var json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<VersionDto>(json);
}
```

### Game State
**GET /api/v1/game/state**

Get current game state including tick, wealth, and colonist count.

**Response:**
```json
{
  "gameTick": 15420,
  "colonyWealth": 12500.5,
  "colonistCount": 8,
  "storyteller": "Cassandra Classic",
  "lastUpdate": "2023-10-15T14:30:00Z"
}
```

**With ETag Example:**
```csharp
public async Task<GameStateDto> GetGameStateAsync(string previousEtag = null)
{
    using var client = new HttpClient();
    var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8765/api/v1/game/state");
    
    if (!string.IsNullOrEmpty(previousEtag))
    {
        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(previousEtag));
    }
    
    var response = await client.SendAsync(request);
    
    if (response.StatusCode == HttpStatusCode.NotModified)
        return null; // Data unchanged
        
    response.EnsureSuccessStatusCode();
    
    var json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<GameStateDto>(json);
}
```

### Colonists
**GET /api/v1/colonists**

Get list of all colonists with basic information.

**Response:**
```json
[
  {
    "id": 1,
    "name": "John",
    "gender": "Male", 
    "age": 28,
    "health": 0.95,
    "mood": 0.72,
    "position": {
      "x": 15,
      "y": 0,
      "z": 22
    }
  }
]
```

**GET /api/v1/colonists/{id}**

Get detailed information about a specific colonist.

**Parameters:**
- `id` (path) - Colonist ID

**Response:**
```json
{
  "id": 1,
  "name": "John",
  "gender": "Male",
  "age": 28,
  "health": 0.95,
  "mood": 0.72,
  "position": {
    "x": 15,
    "y": 0, 
    "z": 22
  }
}
```

**Fields Filtering Example:**
```csharp
// Only get name and health fields
var url = "http://localhost:8765/api/v1/colonists?fields=name,health";
var response = await client.GetAsync(url);

// Response will only contain:
// [{"name": "John", "health": 0.95}, ...]
```

### World Information
**GET /api/v1/world/info**

Get world information including seed and planet details.

**Response:**
```json
{
  "name": "My World",
  "seed": "abc123",
  "coverage": "0.3", 
  "planetName": "Earth"
}
```

**GET /api/v1/world/maps**

Get list of all maps in the world.

**Response:**
```json
[
  {
    "id": 1,
    "name": "Home Map",
    "tile": 125,
    "biome": "Temperate Forest",
    "size": "250x250"
  }
]
```

## Real-time Events (Server-Sent Events)

**GET /api/v1/events**

Connect via Server-Sent Events for real-time game updates. This endpoint provides a persistent connection that streams game events to the client.

**JavaScript Example:**
```javascript
const eventSource = new EventSource('http://localhost:8765/api/v1/events');

eventSource.addEventListener('connected', function(event) {
    console.log('SSE connection established');
});

eventSource.addEventListener('gameState', function(event) {
    const gameState = JSON.parse(event.data);
    console.log('Initial game state:', gameState);
});

eventSource.addEventListener('gameUpdate', function(event) {
    const gameState = JSON.parse(event.data);
    console.log('Game update:', gameState);
});

eventSource.addEventListener('heartbeat', function(event) {
    console.log('Heartbeat received');
});

eventSource.onerror = function(event) {
    console.error('SSE error:', event);
};
```

**Event Types:**
- `connected` - Connection established
- `gameState` - Initial complete game state
- `gameUpdate` - Periodic game state updates
- `heartbeat` - Connection keep-alive (every 30 seconds)
