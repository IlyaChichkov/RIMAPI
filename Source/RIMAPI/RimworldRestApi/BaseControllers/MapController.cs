using System.Collections.Generic;
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
        public async Task GetGameState(HttpListenerContext context)
        {
            var result = _mapService.GetMaps();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/things")]
        public async Task GetMapThings(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapThings(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/things-at")]
        public async Task GetThingsAtCell(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ThingsAtCellRequestDto>();
            var result = _mapService.GetThingsAtCell(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/things/radius")]
        public async Task GetMapThingsInRadius(HttpListenerContext context)
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
        public async Task GetMapPlants(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapPlants(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/weather")]
        [EndpointMetadata("Get weather on the map")]
        public async Task GetMapWeather(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetWeather(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/power/info")]
        public async Task GetMapPowerInfo(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapPowerInfo(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/animals")]
        [EndpointMetadata("Get animals on the map")]
        public async Task GetMapAnimals(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapAnimals(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/creatures/summary")]
        public async Task GetMapCreaturesSummary(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapCreaturesSummary(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/farm/summary")]
        public async Task GetMapFarmSummary(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GenerateFarmSummary(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/zone/growing")]
        public async Task GetMapGrowingZoneById(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var zoneId = RequestParser.GetMapId(context);
            var result = _mapService.GetGrowingZoneById(mapId, zoneId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/zones")]
        [EndpointMetadata("Get zones on the map")]
        public async Task GetMapZones(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapZones(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/rooms")]
        public async Task GetMapRooms(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapRooms(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/terrain")]
        public async Task GetMapTerrain(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapTerrain(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/buildings")]
        public async Task GetMapBuildings(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.GetMapBuildings(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/building/info")]
        [EndpointMetadata("Get building info")]
        public async Task GetBuildingInfo(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _buildingService.GetBuildingInfo(mapId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/weather/change")]
        [EndpointMetadata("Set weather on the map")]
        public async Task SetWeather(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var defName = RequestParser.GetStringParameter(context, "name");
            var result = _mapService.SetWeather(mapId, defName);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/destroy/corpses")]
        public async Task DestroyCorpses(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.DestroyCorpses(mapId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/destroy/forbidden")]
        public async Task DestroyForbiddenItems(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _mapService.DestroyForbiddenItems(mapId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/destroy/rect")]
        public async Task DestroyThingsInRect(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DestroyRectRequestDto>();
            var result = _mapService.DestroyThingsInRect(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/repair/positions")]
        public async Task RepairAtPositions(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RepairPositionsRequestDto>();
            var result = _mapService.RepairThingsAtPositions(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/repair/rect")]
        public async Task RepairInRect(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RepairRectRequestDto>();
            var result = _mapService.RepairThingsInRect(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/droppod")]
        public async Task SpawnDropPod(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SpawnDropPodRequestDto>();
            var result = _mapService.SpawnDropPod(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/pawns")]
        public async Task GetPawnsOnMap(HttpListenerContext context)
        {
            int mapId = RequestParser.GetMapId(context);
            var result = _pawnInfoService.GetPawnsOnMap(mapId);
            await context.SendJsonResponse(result);
        }
    }
}
