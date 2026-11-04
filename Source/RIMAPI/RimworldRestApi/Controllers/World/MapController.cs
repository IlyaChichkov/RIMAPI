using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class MapController
    {
        private readonly IMapService _mapService;
        private readonly IBuildingService _buildingService;
        private readonly IPawnInfoService _pawnInfoService;

        public MapController(IMapService mapService, IBuildingService buildingService,
                             IPawnInfoService pawnInfoService)
        {
            _mapService = mapService;
            _buildingService = buildingService;
            _pawnInfoService = pawnInfoService;
        }

        [Get("/api/v1/maps")]
        [EndpointMetadata("Get all generated maps list the in game session")]
        public async Task GetGameState(RimApiContext context)
        {
            var result = _mapService.GetMaps();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/things")]
        public async Task GetMapThings(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapThings(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/things-at")]
        public async Task GetThingsAtCell(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<ThingsAtCellRequestDto>();
            var result = _mapService.GetThingsAtCell(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/things/radius")]
        public async Task GetMapThingsInRadius(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var x = RequestParser.GetIntParameter(context, "x");
            var z = RequestParser.GetIntParameter(context, "z");
            var radius = RequestParser.GetIntParameter(context, "radius");

            var result = _mapService.GetMapThingsInRadius(mapId, x, z, radius);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/plants")]
        [EndpointMetadata("Get all plants (trees, bushes, crops) on the map")]
        public async Task GetMapPlants(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapPlants(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/weather")]
        [EndpointMetadata("Get weather on the map")]
        public async Task GetMapWeather(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetWeather(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/power/info")]
        public async Task GetMapPowerInfo(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapPowerInfo(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/ore")]
        public async Task GetMapOre(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapOre(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/animals")]
        [EndpointMetadata("Get animals on the map")]
        public async Task GetMapAnimals(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapAnimals(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/creatures/summary")]
        public async Task GetMapCreaturesSummary(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapCreaturesSummary(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/farm/summary")]
        public async Task GetMapFarmSummary(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GenerateFarmSummary(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/zone/growing")]
        public async Task GetMapGrowingZoneById(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var zoneId = RequestParser.GetMapId(context);
            var result = _mapService.GetGrowingZoneById(mapId, zoneId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/zone/growing")]
        [EndpointMetadata("Create a new growing zone with the specified plant and cells")]
        public async Task CreateGrowingZone(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<CreateGrowingZoneRequestDto>();
            var result = _mapService.CreateGrowingZone(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/zones")]
        [EndpointMetadata("Get zones on the map")]
        public async Task GetMapZones(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapZones(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/rooms")]
        public async Task GetMapRooms(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapRooms(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/terrain")]
        public async Task GetMapTerrain(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapTerrain(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/fog-grid")]
        public async Task GetMapFogGrid(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetFogGrid(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/buildings")]
        public async Task GetMapBuildings(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapBuildings(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/building/info")]
        [EndpointMetadata("Get building info")]
        public async Task GetBuildingInfo(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _buildingService.GetBuildingInfo(mapId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/building/power")]
        [EndpointMetadata("Toggle power on/off for a flickable building")]
        public async Task SetBuildingPower(RimApiContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "buildingId");
            var powerOn = RequestParser.GetBooleanParameter(context, "powerOn");
            var result = _buildingService.SetBuildingPower(buildingId, powerOn);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/weather/change")]
        [EndpointMetadata("Set weather on the map")]
        public async Task SetWeather(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var defName = RequestParser.GetStringParameter(context, "name");
            var result = _mapService.SetWeather(mapId, defName);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/destroy/corpses")]
        public async Task DestroyCorpses(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.DestroyCorpses(mapId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/destroy/forbidden")]
        public async Task DestroyForbiddenItems(RimApiContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.DestroyForbiddenItems(mapId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/destroy/rect")]
        public async Task DestroyThingsInRect(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<DestroyRectRequestDto>();
            var result = _mapService.DestroyThingsInRect(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/repair/positions")]
        public async Task RepairAtPositions(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<RepairPositionsRequestDto>();
            var result = _mapService.RepairThingsAtPositions(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/repair/rect")]
        public async Task RepairInRect(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<RepairRectRequestDto>();
            var result = _mapService.RepairThingsInRect(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/droppod")]
        public async Task SpawnDropPod(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<SpawnDropPodRequestDto>();
            var result = _mapService.SpawnDropPod(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/pawns")]
        public async Task GetPawnsOnMap(RimApiContext context)
        {
            int mapId = RequestParser.GetMapId(context);
            var result = _pawnInfoService.GetPawnsOnMap(mapId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/zone/stockpile")]
        [EndpointMetadata("Create a new stockpile zone on the map")]
        public async Task CreateStockpile(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<CreateStockpileRequestDto>();
            var result = _mapService.CreateStockpile(body);
            await context.SendJsonResponse(result);
        }

        [Delete("/api/v1/map/zone/stockpile/delete")]
        [EndpointMetadata("Delete a stockpile zone by zone ID")]
        public async Task DeleteStockpile(RimApiContext context)
        {
            var zoneId = RequestParser.GetIntParameter(context, "zone_id");
            var result = _mapService.DeleteStockpile(zoneId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/zone/stockpile/update")]
        [EndpointMetadata("Update stockpile zone parameters by zone ID")]
        public async Task UpdateStockpile(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<UpdateStockpileRequestDto>();
            var result = _mapService.UpdateStockpile(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/datetime")]
        [EndpointMetadata("Get in-game date and time")]
        public async Task GetCurrentMapDatetime(RimApiContext context)
        {
            var result = _mapService.GetCurrentMapDatetime();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/datetime/tile")]
        [EndpointMetadata("Get in-game date and time in global map tile")]
        public async Task GetWorldTileDatetime(RimApiContext context)
        {
            var tileId = RequestParser.GetIntParameter(context, "tile_id");
            var result = _mapService.GetWorldTileDatetime(tileId);
            await context.SendJsonResponse(result);
        }

    }
}
