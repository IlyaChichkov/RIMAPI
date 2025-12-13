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

        public MapController(IMapService mapService, IBuildingService buildingService)
        {
            _mapService = mapService;
            _buildingService = buildingService;
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
    }
}
