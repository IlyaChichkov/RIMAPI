# Changelog
## v1.5.0

Impliment API server launch at game menu screen (previously lauched when game map loaded).

Fix GetColonist, GetColonistDetailed output when colonist with Id not found to display error.

Add new endpoints:

[POST] /api/v1/item/spawn
[POST] /api/v1/game/save
[POST] /api/v1/game/load
[POST] /api/v1/game/start/devquick
[POST] /api/v1/game/start
[GET] /api/v1/game/settings

Update README.

## v1.4.1

Fix UI blinking during texture make readable

## v1.4.0

Author: IlyaChichkov

Add new endpoints:

[POST] /api/v1/game/speed
[POST] /api/v1/game/select-area
[GET] /api/v1/map/things-at

Author: braasdas

This patch implements high-performance endpoints and data optimizations required for a real-time "Live Optical View" web interface. The primary goals were reducing network bandwidth (via RLE compression) and minimizing game thread impact (via caching).

1. NEW API ENDPOINTS
--------------------
[GET] /api/v1/map/terrain
- Purpose: Fetches the entire map's terrain and floor grid.
- Optimization: Uses custom Run-Length Encoding (RLE) to compress the grid data. This reduces payload size by ~90% for typical maps, making full map transmission viable over the network.

[GET] /api/v1/colonists/positions
- Purpose: A lightweight endpoint returning only Pawn ID, MapID, X, and Z coordinates.
- Optimization: Designed for high-frequency polling (e.g., 10-60Hz). Implements 0.1s server-side caching to prevent flooding the main game thread.

[GET] /api/v1/terrain/image
- Purpose: Fetches the texture/icon for specific terrain or floor defs (e.g., "SandstoneTile", "CarpetRed").
- Why: Required for the client to reconstruct the map visually.

[GET] /api/v1/map/plants
- Purpose: Fast retrieval of all vegetation (trees, crops). separated from general "things" to allow different polling rates.

[GET] /api/v1/map/things/radius
- Purpose: Efficiently queries items/buildings only within a specific circle. Useful for culling or "fog of war" logic.

2. LOGIC & HELPER IMPROVEMENTS
------------------------------
TextureHelper.cs (Major Fixes)
- Problem: Many buildings (Walls, Vents, Coolers) do not have a standard `uiIcon`.
- Fix: Added deep lookup logic to check `graphicData`, `graphic.MatSingle`, and `graphic.MatSouth`. This ensures almost all buildings now return a valid base64 image.
- Added fallback case-insensitive search for DefNames to handle minor typo/mod inconsistencies.

MapHelper.cs
- Added the RLE compression logic for the Terrain/Floor grids.
- Separated "Natural Terrain" (soil, stone) from "Constructed Floors" (wood, tile) into two distinct layers for better rendering control.

ResourcesHelper.cs
- Updated `BuildingDto` and `ItemsDto` to include `Rotation` and `Size` (x, z).
- Why: Critical for the client to correctly orient non-square objects (e.g., Beds, Tables) which were previously rendering as 1x1 squares or unrotated images.

3. NEW DATA MODELS
------------------
- MapTerrainDto: Handles the compressed grid arrays and palette lookups.
- PawnPositionDto: Minimalist structure for the fast position endpoint.

These changes are largely additive and designed to run alongside existing logic without breaking current endpoints. The modifications to `TextureHelper` are strictly improvements to robustness and should benefit the entire API.

## v1.3.0

Add examples and description for endpoints in documentation

Insert them into auto generated api.md by macroses from api.yml

Add option to use json body for endpoints that accept parameters:
- /api/v1/dev/console
- /api/v1/colonist/time-assignment

Add new endpoints:
[GET]

- /api/v1/world/caravans
- /api/v1/world/settlements
- /api/v1/world/caravans
- /api/v1/world/tile

[POST]
- /api/v1/pawn/edit

Fixes:
- TraitDefDto empty label, description
- /api/v1/colonist/body/image returned GetColonistInventory
- change "throw new Exception" to "return ApiResult.Fail"
- /api/v1/dev/console didn't have message parameter

## v1.2.2

Fix GetItemImageByName

## v1.2.1

Remove /api/v1/pawn/portrait/image endpoint duplicate
Fix screen blinking when get rendered texture

## v1.2.0

### Add endpoint:
[GET]
- /api/v1/def/all
[POST]
- /api/v1/game/send/letter

### Change endpoint path:
- /api/v1/building/info -> /api/v1/map/building/info
- /api/v1/change/weather -> /api/v1/map/weather/change

## v1.1.0

### Add endpoint:
[GET]
- /api/v1/faction/player
- /api/v1/faction/
- /api/v1/faction/def
- /api/v1/faction/relations-with
- /api/v1/faction/relations
[POST]
- /api/v1/faction/change/goodwill
- /api/v1/change/weather

### Add caching service

Test performance of GET /colonists/detailed endpoint no/with caching, results:

- Speed acceleration of 16-21%
- Improved stability (0 vs 9 failures)
- Reduction of peak delays by 72%

### Fix: Add endpoints from v0.5.6
- /api/v1/resources/storages/summary
- /api/v1/select
- /api/v1/trait-def
- /api/v1/time-assignments
- /api/v1/colonist/time-assignment
- /api/v1/outfits
- /api/v1/work-list
- /api/v1/colonist/work-priority
- /api/v1/colonists/work-priority
- /api/v1/jobs/make/equip

Thanks to @braasdas and his [RatLab](https://github.com/braasdas/ratlab-mod-github) mod

## v1.0.0

Complete architectural rewrite to use dependency injection (DI) container system

Updates:
- Added automatic service discovery and lifetime management (singleton/transient)
- Introduced constructor injection for all service dependencies
- Added automatic extension discovery via reflection scanning
- Implemented attribute-based routing with automatic controller registration
- Added support auto-routed controllers
- Created documentation service with auto-generated API documentation

## v0.5.6

Fix /api/v1/resources/stored

## v0.5.5

Add endpoint:
[GET]
- /api/v1/materials-atlas
[POST]
- /api/v1/dev/console
- /api/v1/materials-atlas/clear
- /api/v1/stuff/color
- /api/v1/item/image

Minor fixes

## v0.5.4

Add SSE broadcast:
- message_received
- letter_received
- make_recipe_product
- unfinished_destroyed
- date_changed

Updated SSE broadcast:
- colonist_ate

Add endpoint:
- /api/v1/map/rooms
- /api/v1/time-assignments
- /api/v1/colonist/time-assignment
- /api/v1/outfits

SSE service refactoring & fixes
Update debug logging class
Fix loggingLevel config value wasn't save in Scribe_Values 
Add example script for colony food analysis 

## v0.5.3

Add endpoint:
- /api/v1/jobs/make/equip
- /api/v1/pawn/portrait/image
- /api/v1/colonist/work-priority
- /api/v1/colonists/work-priority
- /api/v1/work-list

Optimize resources Dto
Update BaseController CORS header handling

## v0.5.2
Add endpoint:
- /api/v1/resources/stored

Update resources Dto

## v0.5.1
Update skills Dto

## v0.5.0

Add more endpoints
[POST]
- /api/v1/deselect
- /api/v1/deselect
- /api/v1/deselect
- /api/v1/select
- /api/v1/open-tab

[GET]
- /api/v1/map/zones
- /api/v1/map/buildings
- /api/v1/building/info

Update headiffs data in 
- /api/v1/colonists/detailed
- /api/v1/colonist/detailed

Add SSE endpoint:
- 'colonist_ate'

Add SSE client for testing

## v0.4.4

Add camera stream (default: localhost:5001)
Add camera stream endpoints to start, stop, setup, get status

## v0.4.3

Add quests and incidents endpoints
Add pawn opinion about pawn endpoint
Update colonist detailed data

Steam version updated

## v0.4.0

Update research endpoints
Improved dashboard example: https://github.com/IlyaChichkov/rimapi-dashboard

## v0.3.0

Update mod settings

## v0.2.0

Update README, Licence, Github CI/CD
Fix exception handling

## v0.1.0

Add basic endpoints
Add docs
