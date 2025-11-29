# RimWorld REST API Documentation

**Generated**: 2025-11-30 16:43:05 UTC  
**Version**: 1.0.0  

## Core API

Built-in RimWorld REST API endpoints

### General

#### `POST /api/v1/camera/change/zoom`

Change game camera zoom</br>[CameraController.ChangeZoom](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+ChangeZoom&type=code "ChangeZoom")

---

#### `POST /api/v1/camera/change/position`

Change game camera position</br>[CameraController.MoveToPosition](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+MoveToPosition&type=code "MoveToPosition")

---

#### `POST /api/v1/stream/start`

Start game camera stream</br>[CameraController.PostStreamStart](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+PostStreamStart&type=code "PostStreamStart")

---

#### `POST /api/v1/stream/stop`

Stop game camera stream</br>[CameraController.PostStreamStop](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+PostStreamStop&type=code "PostStreamStop")

---

#### `POST /api/v1/stream/setup`

Set game camera stream configuration</br>[CameraController.PostStreamSetup](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+PostStreamSetup&type=code "PostStreamSetup")

---

#### `GET /api/v1/stream/status`

Get game camera stream status</br>[CameraController.GetStreamStatus](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29CameraController%5C.cs%24%2F+GetStreamStatus&type=code "GetStreamStatus")

---

#### `POST /api/v1/dev/console`

Send message to the debug console</br>[DevToolsController.PostConsoleAction](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DevToolsController%5C.cs%24%2F+PostConsoleAction&type=code "PostConsoleAction")

---

#### `POST /api/v1/dev/stuff/color`

Change stuff color</br>[DevToolsController.PostStuffColor](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DevToolsController%5C.cs%24%2F+PostStuffColor&type=code "PostStuffColor")

---

#### `GET /api/v1/version`

Get versions of: game, mod, API</br>[GameController.GetVersion](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetVersion&type=code "GetVersion")

---

#### `GET /api/v1/mods/info`

Get list of active mods</br>[GameController.GetModsInfo](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetModsInfo&type=code "GetModsInfo")

---

#### `POST /api/v1/deselect`

Clear game selection</br>[GameController.DeselectAll](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+DeselectAll&type=code "DeselectAll")

---

#### `POST /api/v1/open-tab`

Open interface tab</br>[GameController.OpenTab](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+OpenTab&type=code "OpenTab")

---

### DocumentationController

#### `GET /api/v1/core/docs/export`

[DocumentationController.ExportCoreDocumentation](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+ExportCoreDocumentation&type=code "ExportCoreDocumentation")

---

#### `GET /api/v1/docs`

[DocumentationController.GetDocumentation](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+GetDocumentation&type=code "GetDocumentation")

---

#### `GET /api/v1/docs/extensions/{extensionId}`

[DocumentationController.GetExtensionDocumentation](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+GetExtensionDocumentation&type=code "GetExtensionDocumentation")

**Parameters:**

| Name | Type | Required | Description | Example |
|------|------|:--------:|-------------|---------|
| `extensionId` | `String` | ✅ | Unique identifier | *N/A* |

---

#### `GET /api/v1/docs/health`

[DocumentationController.GetHealth](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+GetHealth&type=code "GetHealth")

---

#### `GET /api/v1/docs/export`

[DocumentationController.ExportDocumentation](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DocumentationController%5C.cs%24%2F+ExportDocumentation&type=code "ExportDocumentation")

**Parameters:**

| Name | Type | Required | Description | Example |
|------|------|:--------:|-------------|---------|
| `saveFile` | `Boolean` | ❌ | Parameter: saveFile | *N/A* |

---

### DevToolsController

#### `GET /api/v1/dev/materials-atlas`

[DevToolsController.GetMaterialsAtlasList](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DevToolsController%5C.cs%24%2F+GetMaterialsAtlasList&type=code "GetMaterialsAtlasList")

---

#### `POST /api/v1/dev/materials-atlas/clear`

[DevToolsController.PostMaterialsAtlasPoolClear](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29DevToolsController%5C.cs%24%2F+PostMaterialsAtlasPoolClear&type=code "PostMaterialsAtlasPoolClear")

---

### FactionController

#### `GET /api/v1/factions`

[FactionController.GetCurrentMapDatetime](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29FactionController%5C.cs%24%2F+GetCurrentMapDatetime&type=code "GetCurrentMapDatetime")

---

### GameController

#### `GET /api/v1/game/state`

[GameController.GetGameState](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetGameState&type=code "GetGameState")

---

#### `GET /api/v1/datetime`

[GameController.GetCurrentMapDatetime](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetCurrentMapDatetime&type=code "GetCurrentMapDatetime")

---

#### `GET /api/v1/datetime/tile`

[GameController.GetWorldTileDatetime](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetWorldTileDatetime&type=code "GetWorldTileDatetime")

---

#### `GET /api/v1/def/all`

[GameController.GetAllDefs](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameController%5C.cs%24%2F+GetAllDefs&type=code "GetAllDefs")

---

### GameEventsController

#### `GET /api/v1/quests`

[GameEventsController.GetQuestsData](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameEventsController%5C.cs%24%2F+GetQuestsData&type=code "GetQuestsData")

---

#### `GET /api/v1/incidents`

[GameEventsController.GetIncidentsData](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29GameEventsController%5C.cs%24%2F+GetIncidentsData&type=code "GetIncidentsData")

---

### ImageController

#### `GET /api/v1/item/image`

[ImageController.GetItemImage](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ImageController%5C.cs%24%2F+GetItemImage&type=code "GetItemImage")

---

#### `POST /api/v1/item/image`

[ImageController.SetItemImage](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ImageController%5C.cs%24%2F+SetItemImage&type=code "SetItemImage")

---

### MapController

#### `GET /api/v1/maps`

[MapController.GetGameState](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetGameState&type=code "GetGameState")

---

#### `GET /api/v1/map/things`

[MapController.GetMapThings](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapThings&type=code "GetMapThings")

---

#### `GET /api/v1/map/weather`

[MapController.GetMapWeather](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapWeather&type=code "GetMapWeather")

---

#### `GET /api/v1/map/power/info`

[MapController.GetMapPowerInfo](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapPowerInfo&type=code "GetMapPowerInfo")

---

#### `GET /api/v1/map/animals`

[MapController.GetMapAnimals](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapAnimals&type=code "GetMapAnimals")

---

#### `GET /api/v1/map/creatures/summary`

[MapController.GetMapCreaturesSummary](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapCreaturesSummary&type=code "GetMapCreaturesSummary")

---

#### `GET /api/v1/map/farm/summary`

[MapController.GetMapFarmSummary](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapFarmSummary&type=code "GetMapFarmSummary")

---

#### `GET /api/v1/map/zone/growing`

[MapController.GetMapGrowingZoneById](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapGrowingZoneById&type=code "GetMapGrowingZoneById")

---

#### `GET /api/v1/map/zones`

[MapController.GetMapZones](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapZones&type=code "GetMapZones")

---

#### `GET /api/v1/map/rooms`

[MapController.GetMapRooms](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapRooms&type=code "GetMapRooms")

---

#### `GET /api/v1/map/buildings`

[MapController.GetMapBuildings](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetMapBuildings&type=code "GetMapBuildings")

---

#### `GET /api/v1/building/info`

[MapController.GetBuildingInfo](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29MapController%5C.cs%24%2F+GetBuildingInfo&type=code "GetBuildingInfo")

---

### PawnController

#### `GET /api/v1/colonists`

[PawnController.GetColonists](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetColonists&type=code "GetColonists")

---

#### `GET /api/v1/colonist`

[PawnController.GetColonist](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetColonist&type=code "GetColonist")

---

#### `GET /api/v1/colonists/detailed`

[PawnController.GetColonistsDetailed](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetColonistsDetailed&type=code "GetColonistsDetailed")

---

#### `GET /api/v1/colonist/detailed`

[PawnController.GetResearchProGetColonistDetailedgress](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetResearchProGetColonistDetailedgress&type=code "GetResearchProGetColonistDetailedgress")

---

#### `GET /api/v1/colonist/opinion-about`

[PawnController.GetPawnOpinionAboutPawn](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetPawnOpinionAboutPawn&type=code "GetPawnOpinionAboutPawn")

---

#### `GET /api/v1/colonist/inventory`

[PawnController.GetColonistInventory](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetColonistInventory&type=code "GetColonistInventory")

---

#### `GET /api/v1/colonist/body/image`

[PawnController.GetPawnBodyImage](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetPawnBodyImage&type=code "GetPawnBodyImage")

---

#### `GET /api/v1/pawn/portrait/image`

[PawnController.GetPawnPortraitImage](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29PawnController%5C.cs%24%2F+GetPawnPortraitImage&type=code "GetPawnPortraitImage")

---

### ResearchController

#### `GET /api/v1/research/progress`

[ResearchController.GetResearchProgress](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchProgress&type=code "GetResearchProgress")

---

#### `GET /api/v1/research/finished`

[ResearchController.GetResearchFinished](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchFinished&type=code "GetResearchFinished")

---

#### `GET /api/v1/research/tree`

[ResearchController.GetResearchTree](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchTree&type=code "GetResearchTree")

---

#### `GET /api/v1/research/project`

[ResearchController.GetResearchProjectByName](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchProjectByName&type=code "GetResearchProjectByName")

---

#### `GET /api/v1/research/summary`

[ResearchController.GetResearchSummary](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ResearchController%5C.cs%24%2F+GetResearchSummary&type=code "GetResearchSummary")

---

### ThingController

#### `GET /api/v1/resources/summary`

[ThingController.GetResourcesSummary](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ThingController%5C.cs%24%2F+GetResourcesSummary&type=code "GetResourcesSummary")

---

#### `GET /api/v1/resources/stored`

[ThingController.GetResourcesStored](https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29ThingController%5C.cs%24%2F+GetResourcesStored&type=code "GetResourcesStored")

---

