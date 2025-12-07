# RimWorld REST API Documentation

**Generated**: 2025-12-06 18:48:47 UTC  
**Version**: 1.2.1  

## Core API

Built-in RimWorld REST API endpoints

### General

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/camera/change/zoom</code></div>
</div>
</div>

Change game camera zoom

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+ChangeZoom&type=code" class="doc-github-link">
CameraController.ChangeZoom
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/camera/change/position</code></div>
</div>
</div>

Change game camera position

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+MoveToPosition&type=code" class="doc-github-link">
CameraController.MoveToPosition
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/stream/start</code></div>
</div>
</div>

Start game camera stream

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+PostStreamStart&type=code" class="doc-github-link">
CameraController.PostStreamStart
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/stream/stop</code></div>
</div>
</div>

Stop game camera stream

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+PostStreamStop&type=code" class="doc-github-link">
CameraController.PostStreamStop
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/stream/setup</code></div>
</div>
</div>

Set game camera stream configuration

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+PostStreamSetup&type=code" class="doc-github-link">
CameraController.PostStreamSetup
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/stream/status</code></div>
</div>
</div>

Get game camera stream status

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+GetStreamStatus&type=code" class="doc-github-link">
CameraController.GetStreamStatus
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/dev/console</code></div>
</div>
</div>

Send message to the debug console

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DevToolsController%5C.cs%24%2F+PostConsoleAction&type=code" class="doc-github-link">
DevToolsController.PostConsoleAction
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/dev/stuff/color</code></div>
</div>
</div>

Change stuff color

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DevToolsController%5C.cs%24%2F+PostStuffColor&type=code" class="doc-github-link">
DevToolsController.PostStuffColor
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/cache/status</code></div>
</div>
</div>

Get cache status

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetCacheStatus&type=code" class="doc-github-link">
GameController.GetCacheStatus
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/cache/stats</code></div>
</div>
</div>

Get cache statistics

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetCacheStats&type=code" class="doc-github-link">
GameController.GetCacheStats
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/cache/clear</code></div>
</div>
</div>

Clear cache

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+ClearCache&type=code" class="doc-github-link">
GameController.ClearCache
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/version</code></div>
</div>
</div>

Get versions of: game, mod, API

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetVersion&type=code" class="doc-github-link">
GameController.GetVersion
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/mods/info</code></div>
</div>
</div>

Get list of active mods

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetModsInfo&type=code" class="doc-github-link">
GameController.GetModsInfo
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/select</code></div>
</div>
</div>

Select game object

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+Select&type=code" class="doc-github-link">
GameController.Select
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/deselect</code></div>
</div>
</div>

Clear game selection

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+DeselectAll&type=code" class="doc-github-link">
GameController.DeselectAll
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/open-tab</code></div>
</div>
</div>

Open interface tab

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+OpenTab&type=code" class="doc-github-link">
GameController.OpenTab
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/datetime</code></div>
</div>
</div>

Get in-game date and time

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetCurrentMapDatetime&type=code" class="doc-github-link">
GameController.GetCurrentMapDatetime
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/datetime/tile</code></div>
</div>
</div>

Get in-game date and time in global map tile

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetWorldTileDatetime&type=code" class="doc-github-link">
GameController.GetWorldTileDatetime
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/def/all</code></div>
</div>
<div class="doc-api-tags">
<span class="doc-api-tag doc-api-tag-unstable"><code>Unstable</code></span>
</div>
</div>

Get in-game date and time in global map tile

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetAllDefs&type=code" class="doc-github-link">
GameController.GetAllDefs
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/incidents</code></div>
</div>
</div>

Get map incidents

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameEventsController%5C.cs%24%2F+GetIncidentsData&type=code" class="doc-github-link">
GameEventsController.GetIncidentsData
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/lords</code></div>
</div>
</div>

Get lords on map (AI raid managing objects)

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameEventsController%5C.cs%24%2F+GetLordsData&type=code" class="doc-github-link">
GameEventsController.GetLordsData
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/incident/trigger</code></div>
</div>
</div>

Trigger game incident

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameEventsController%5C.cs%24%2F+TriggerIncident&type=code" class="doc-github-link">
GameEventsController.TriggerIncident
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/item/image</code></div>
</div>
</div>

Get item's texture image in base64 format

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ImageController%5C.cs%24%2F+GetItemImage&type=code" class="doc-github-link">
ImageController.GetItemImage
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/item/image</code></div>
</div>
</div>

Get item's texture image in base64 format

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ImageController%5C.cs%24%2F+SetItemImage&type=code" class="doc-github-link">
ImageController.SetItemImage
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/maps</code></div>
</div>
</div>

Get all generated maps list the in game session

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetGameState&type=code" class="doc-github-link">
MapController.GetGameState
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/weather</code></div>
</div>
</div>

Get weather on the map

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapWeather&type=code" class="doc-github-link">
MapController.GetMapWeather
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/animals</code></div>
</div>
</div>

Get animals on the map

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapAnimals&type=code" class="doc-github-link">
MapController.GetMapAnimals
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/zones</code></div>
</div>
</div>

Get zones on the map

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapZones&type=code" class="doc-github-link">
MapController.GetMapZones
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/building/info</code></div>
</div>
<div class="doc-api-tags">
<span class="doc-api-tag doc-api-tag-unstable"><code>Unstable</code></span>
</div>
</div>

Get building info

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetBuildingInfo&type=code" class="doc-github-link">
MapController.GetBuildingInfo
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/map/weather/change</code></div>
</div>
</div>

Set weather on the map

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+SetWeather&type=code" class="doc-github-link">
MapController.SetWeather
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/colonist/time-assignment</code></div>
</div>
</div>

Schedule pawn assignment during provided hour

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+SetTimeAssignment&type=code" class="doc-github-link">
PawnController.SetTimeAssignment
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/colonist/work-priority</code></div>
</div>
</div>

Set pawn work priorities

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+SetColonistWorkPriority&type=code" class="doc-github-link">
PawnController.SetColonistWorkPriority
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/colonists/work-priority</code></div>
</div>
</div>

Set multiple work priorities to several pawns

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+SetColonistsWorkPriority&type=code" class="doc-github-link">
PawnController.SetColonistsWorkPriority
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/jobs/make/equip</code></div>
</div>
</div>

Set multiple work priorities to several pawns

<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+MakeJobEquip&type=code" class="doc-github-link">
PawnController.MakeJobEquip
</a>
</div>

---

### DocumentationController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/core/docs/export</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+ExportCoreDocumentation&type=code" class="doc-github-link">
DocumentationController.ExportCoreDocumentation
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/docs</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+GetDocumentation&type=code" class="doc-github-link">
DocumentationController.GetDocumentation
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/docs/extensions/{extensionId}</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+GetExtensionDocumentation&type=code" class="doc-github-link">
DocumentationController.GetExtensionDocumentation
</a>
</div>

**Parameters:**

| Name | Type | Required | Description | Example |
|------|------|:--------:|-------------|---------|
| `extensionId` | `String` | ✅ | Unique identifier | *N/A* |

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/docs/health</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+GetHealth&type=code" class="doc-github-link">
DocumentationController.GetHealth
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/docs/export</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+ExportDocumentation&type=code" class="doc-github-link">
DocumentationController.ExportDocumentation
</a>
</div>

**Parameters:**

| Name | Type | Required | Description | Example |
|------|------|:--------:|-------------|---------|
| `saveFile` | `Boolean` | ❌ | Parameter: saveFile | *N/A* |

---

### ColonistsWorkController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/pawn/portrait/image</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ColonistsWorkController%5C.cs%24%2F+GetPawnPortraitImage&type=code" class="doc-github-link">
ColonistsWorkController.GetPawnPortraitImage
</a>
</div>

---

### DevToolsController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/dev/materials-atlas</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DevToolsController%5C.cs%24%2F+GetMaterialsAtlasList&type=code" class="doc-github-link">
DevToolsController.GetMaterialsAtlasList
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/dev/materials-atlas/clear</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DevToolsController%5C.cs%24%2F+PostMaterialsAtlasPoolClear&type=code" class="doc-github-link">
DevToolsController.PostMaterialsAtlasPoolClear
</a>
</div>

---

### FactionController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/factions</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29FactionController%5C.cs%24%2F+GetFactions&type=code" class="doc-github-link">
FactionController.GetFactions
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/faction/player</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29FactionController%5C.cs%24%2F+GetPlayerFaction&type=code" class="doc-github-link">
FactionController.GetPlayerFaction
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/faction</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29FactionController%5C.cs%24%2F+GetFaction&type=code" class="doc-github-link">
FactionController.GetFaction
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/faction/relation-with</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29FactionController%5C.cs%24%2F+GetFactionRelationWith&type=code" class="doc-github-link">
FactionController.GetFactionRelationWith
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/faction/relations</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29FactionController%5C.cs%24%2F+GetFactionRelations&type=code" class="doc-github-link">
FactionController.GetFactionRelations
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/faction/def</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29FactionController%5C.cs%24%2F+GetFactionDef&type=code" class="doc-github-link">
FactionController.GetFactionDef
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/faction/change/goodwill</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29FactionController%5C.cs%24%2F+ChangeFactionRelationWith&type=code" class="doc-github-link">
FactionController.ChangeFactionRelationWith
</a>
</div>

---

### GameController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/cache/enable</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+EnableCache&type=code" class="doc-github-link">
GameController.EnableCache
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/cache/disable</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+DisableCache&type=code" class="doc-github-link">
GameController.DisableCache
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/game/state</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetGameState&type=code" class="doc-github-link">
GameController.GetGameState
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-post">POST</div>
<div class="doc-api-endpoint"><code>/api/v1/game/send/letter</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+PostLetter&type=code" class="doc-github-link">
GameController.PostLetter
</a>
</div>

---

### GameEventsController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/quests</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameEventsController%5C.cs%24%2F+GetQuestsData&type=code" class="doc-github-link">
GameEventsController.GetQuestsData
</a>
</div>

---

### MapController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/things</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapThings&type=code" class="doc-github-link">
MapController.GetMapThings
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/power/info</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapPowerInfo&type=code" class="doc-github-link">
MapController.GetMapPowerInfo
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/creatures/summary</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapCreaturesSummary&type=code" class="doc-github-link">
MapController.GetMapCreaturesSummary
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/farm/summary</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapFarmSummary&type=code" class="doc-github-link">
MapController.GetMapFarmSummary
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/zone/growing</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapGrowingZoneById&type=code" class="doc-github-link">
MapController.GetMapGrowingZoneById
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/rooms</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapRooms&type=code" class="doc-github-link">
MapController.GetMapRooms
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/map/buildings</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapBuildings&type=code" class="doc-github-link">
MapController.GetMapBuildings
</a>
</div>

---

### PawnController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/colonists</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetColonists&type=code" class="doc-github-link">
PawnController.GetColonists
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/colonist</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetColonist&type=code" class="doc-github-link">
PawnController.GetColonist
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/colonists/detailed</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetColonistsDetailed&type=code" class="doc-github-link">
PawnController.GetColonistsDetailed
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/colonist/detailed</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetResearchProGetColonistDetailedgress&type=code" class="doc-github-link">
PawnController.GetResearchProGetColonistDetailedgress
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/colonist/opinion-about</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetPawnOpinionAboutPawn&type=code" class="doc-github-link">
PawnController.GetPawnOpinionAboutPawn
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/colonist/inventory</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetColonistInventory&type=code" class="doc-github-link">
PawnController.GetColonistInventory
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/colonist/body/image</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetPawnBodyImage&type=code" class="doc-github-link">
PawnController.GetPawnBodyImage
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/pawn/portrait/image</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetPawnPortraitImage&type=code" class="doc-github-link">
PawnController.GetPawnPortraitImage
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/trait-def</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetTraitDef&type=code" class="doc-github-link">
PawnController.GetTraitDef
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/time-assignments</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetTimeAssignmentsList&type=code" class="doc-github-link">
PawnController.GetTimeAssignmentsList
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/outfits</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetOutfits&type=code" class="doc-github-link">
PawnController.GetOutfits
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/work-list</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetWorkList&type=code" class="doc-github-link">
PawnController.GetWorkList
</a>
</div>

---

### ResearchController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/research/progress</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchProgress&type=code" class="doc-github-link">
ResearchController.GetResearchProgress
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/research/finished</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchFinished&type=code" class="doc-github-link">
ResearchController.GetResearchFinished
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/research/tree</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchTree&type=code" class="doc-github-link">
ResearchController.GetResearchTree
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/research/project</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchProjectByName&type=code" class="doc-github-link">
ResearchController.GetResearchProjectByName
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/research/summary</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchSummary&type=code" class="doc-github-link">
ResearchController.GetResearchSummary
</a>
</div>

---

### ThingController

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/resources/summary</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ThingController%5C.cs%24%2F+GetResourcesSummary&type=code" class="doc-github-link">
ThingController.GetResourcesSummary
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/resources/stored</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ThingController%5C.cs%24%2F+GetResourcesStored&type=code" class="doc-github-link">
ThingController.GetResourcesStored
</a>
</div>

---

<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-get">GET</div>
<div class="doc-api-endpoint"><code>/api/v1/resources/storages/summary</code></div>
</div>
</div>



<div class="doc-github-container">
<a href="https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ThingController%5C.cs%24%2F+GetStoragesSummary&type=code" class="doc-github-link">
ThingController.GetStoragesSummary
</a>
</div>

---

