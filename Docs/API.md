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

# RimWorld REST API Documentation

## Base URL
```
http://localhost:8765/api/v1/
```

## Table of Contents
- [Basic Endpoints](#basic-endpoints)
- [Map Endpoints](#map-endpoints)
- [Research Endpoints](#research-endpoints)
- [Colonist Endpoints](#colonist-endpoints)
- [Resource Endpoints](#resource-endpoints)
- [Other Endpoints](#other-endpoints)
- [Real-time Events](#real-time-events)

## Basic Endpoints

### Get API Version
- **GET** `/api/v1/version`
- **Description**: Returns API version and RimWorld compatibility information
- **Response**:
```json
{
  "version": "1.0.0",
  "rimWorldVersion": "1.4.xxxx",
  "modVersion": "1.0.0",
  "apiVersion": "v1"
}
```

### Get Game State
- **GET** `/api/v1/game/state`
- **Description**: Returns current game state including time, weather, and storyteller info
- **Response**:
```json
{
  "gameTime": "5503.5.12",
  "timeSpeed": "Normal",
  "weather": "Clear",
  "temperature": 21.5,
  "storyteller": "Cassandra Classic",
  "difficulty": "Rough"
}
```

### Get Mods Info
- **GET** `/api/v1/mods/info`
- **Description**: Returns list of loaded mods
- **Response**:
```json
{
  "mods": [
    {
      "name": "Core",
      "version": "1.4.xxxx"
    }
  ]
}
```

## Map Endpoints

### Get Available Maps
- **GET** `/api/v1/maps`
- **Description**: Returns list of available maps
- **Response**:
```json
{
  "maps": [
    {
      "id": 1,
      "name": "Main Colony",
      "biome": "Temperate Forest"
    }
  ]
}
```

### Get Weather Info
- **GET** `/api/v1/map/weather`
- **Description**: Returns current weather conditions
- **Response**:
```json
{
  "currentWeather": "Clear",
  "temperature": 21.5,
  "rainRate": 0.0,
  "windSpeed": 0.0
}
```

### Get Power Network Info
- **GET** `/api/v1/map/power/info`
- **Description**: Returns power grid status and consumption
- **Response**:
```json
{
  "powerGenerated": 2500,
  "powerConsumed": 1800,
  "powerStored": 1200,
  "batteryCount": 3
}
```

### Get Map Animals
- **GET** `/api/v1/map/animals`
- **Description**: Returns animals on the map
- **Response**:
```json
{
  "animals": [
    {
      "id": 456,
      "defName": "Deer",
      "label": "deer",
      "position": {"x": 123, "z": 456}
    }
  ]
}
```

### Get Map Things
- **GET** `/api/v1/map/things`
- **Description**: Returns all things/items on the map
- **Response**:
```json
{
  "things": [
    {
      "thingId": 789,
      "defName": "Steel",
      "label": "steel",
      "stackCount": 75
    }
  ]
}
```

### Get Creatures Summary
- **GET** `/api/v1/map/creatures/summary`
- **Description**: Returns summary of all creatures on map by type
- **Response**:
```json
{
  "colonistsCount": 8,
  "prisonersCount": 2,
  "enemiesCount": 0,
  "animalsCount": 15,
  "insectoidsCount": 0,
  "mechanoidsCount": 0
}
```

### Get Farm Summary
- **GET** `/api/v1/map/farm/summary`
- **Description**: Returns farming and crop information
- **Response**:
```json
{
  "totalGrowingZones": 3,
  "totalPlants": 45,
  "totalExpectedYield": 120,
  "cropTypes": [
    {
      "plantDefName": "RicePlant",
      "plantLabel": "rice",
      "totalPlants": 25,
      "expectedYield": 75
    }
  ]
}
```

### Get Growing Zone Details
- **GET** `/api/v1/map/zone/growing?zoneId={id}`
- **Description**: Returns detailed information about a specific growing zone
- **Parameters**: `zoneId` - ID of the growing zone
- **Response**:
```json
{
  "zoneId": 1,
  "plantDefName": "RicePlant",
  "plantCount": 25,
  "growthProgress": 0.75,
  "soilType": "RichSoil"
}
```

## Research Endpoints

### Get Research Progress
- **GET** `/api/v1/research/progress`
- **Description**: Returns current research project and progress
- **Response**:
```json
{
  "currentProject": "Hydroponics",
  "progress": 0.25
}
```

### Get Finished Research
- **GET** `/api/v1/research/finished`
- **Description**: Returns list of completed research projects
- **Response**:
```json
{
  "finishedProjects": ["Stonecutting", "Electricity"]
}
```

### Get Research Tree
- **GET** `/api/v1/research/tree`
- **Description**: Returns complete research tree with all projects
- **Response**:
```json
{
  "projects": [
    {
      "name": "Stonecutting",
      "progress": 1.0,
      "researchPoints": 1000,
      "description": "Allows cutting stone blocks",
      "isFinished": true
    }
  ]
}
```

## Colonist Endpoints

### Get Colonists List
- **GET** `/api/v1/colonists`
- **Description**: Returns basic list of all colonists
- **Response**:
```json
{
  "colonists": [
    {
      "id": 123,
      "name": "John",
      "gender": "Male",
      "age": 32
    }
  ]
}
```

### Get Colonist Details
- **GET** `/api/v1/colonist?id={id}`
- **GET** `/api/v1/colonist/detailed?id={id}`
- **Description**: Returns detailed information about a specific colonist
- **Parameters**: `id` - Colonist ID
- **Response**:
```json
{
  "id": 123,
  "name": "John",
  "health": 1.0,
  "mood": 0.75,
  "skills": [
    {
      "skill": "Shooting",
      "level": 8
    }
  ]
}
```

### Get All Colonists Detailed
- **GET** `/api/v1/colonists/detailed`
- **Description**: Returns detailed information for all colonists
- **Response**: Array of colonist details

### Get Colonist Inventory
- **GET** `/api/v1/colonist/inventory?id={id}`
- **Description**: Returns items carried by a colonist
- **Parameters**: `id` - Colonist ID
- **Response**:
```json
{
  "colonistId": 123,
  "items": [
    {
      "thingId": 456,
      "defName": "Gun_Autopistol",
      "label": "autopistol"
    }
  ]
}
```

## Resource Endpoints

### Get Resources Summary
- **GET** `/api/v1/resources/summary`
- **Description**: Returns comprehensive resource inventory summary
- **Response**:
```json
{
  "totalItems": 150,
  "totalMarketValue": 12500,
  "categories": [
    {
      "category": "Resource",
      "count": 45,
      "marketValue": 5000
    }
  ]
}
```

### Get Storages Summary
- **GET** `/api/v1/resources/storages/summary`
- **Description**: Returns storage zone information and utilization
- **Response**:
```json
{
  "totalStockpiles": 5,
  "totalCells": 200,
  "usedCells": 150,
  "utilizationPercent": 75
}
```

## Other Endpoints

### Get Item Image
- **GET** `/api/v1/item/image?thingId={id}`
- **Description**: Returns item icon information
- **Parameters**: `thingId` - Item ID
- **Response**:
```json
{
  "thingId": 789,
  "defName": "MedicineIndustrial",
  "uiIconPath": "Things/Item/Resource/MedicineIndustrial"
}
```

### Get Colonist Body
- **GET** `/api/v1/colonist/body/image?id={id}`
- **Description**: Returns colonist body/health visualization data
- **Parameters**: `id` - Colonist ID

### Get Date/Time
- **GET** `/api/v1/datetime`
- **Description**: Returns current in-game date and time
- **Response**:
```json
{
  "date": "5503-05-12",
  "time": "14:30",
  "ticks": 123456789
}
```

### Get Factions
- **GET** `/api/v1/factions`
- **Description**: Returns information about all factions
- **Response**:
```json
{
  "factions": [
    {
      "name": "The Tribe",
      "relation": "Hostile",
      "goodwill": -80
    }
  ]
}
```

## Real-time Events

### Server-Sent Events
- **GET** `/api/v1/events`
- **Description**: Establishes a Server-Sent Events connection for real-time game updates
- **Content-Type**: `text/event-stream`
- **Events**: Game state changes, colonist updates, threats, etc.

## Field Filtering

All GET endpoints support field filtering using the `fields` query parameter:

```
GET /api/v1/colonists?fields=id,name,skills
GET /api/v1/resources/summary?fields=totalItems,categories.category,categories.count
```

## Common HTTP Status Codes

- `200 OK` - Successful request
- `304 Not Modified` - Data unchanged (ETag caching)
- `400 Bad Request` - Invalid parameters
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server-side error

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


## Extension System

RIMAPI supports extensions from other mods. Extension endpoints are available under:

```
/api/v1/{extension-id}/{endpoint}
```

### Example Extension Endpoints:

**Jobs Mod:**
```
GET /api/v1/jobs/active      # Get active jobs
GET /api/v1/jobs/queue       # Get job queue
GET /api/v1/jobs/types       # Get available job types
```

**Magic Mod:**
```
GET /api/v1/magic/spells     # Get available spells
GET /api/v1/magic/active     # Get active spell effects
```

### Creating Extensions:

1. **Implement IRimApiExtension**:
```csharp
public class MyModExtension : IRimApiExtension
{
    public string ExtensionId => "mymod";
    public string ExtensionName => "My Mod";
    public string Version => "1.0.0";

    public void RegisterEndpoints(IExtensionRouter router)
    {
        router.Get("data", HandleGetData);
        router.Post("action", HandlePostAction);
    }
}
```

2. **Automatic Discovery**: RIMAPI will automatically find and load your extension
3. **Manual Registration**: If automatic discovery fails, register manually:
```csharp
Find.ApiServer().RegisterExtension(new MyModExtension());
```
