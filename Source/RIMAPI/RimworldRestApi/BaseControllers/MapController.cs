using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Core;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class MapController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public MapController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task GetMaps(HttpListenerContext context)
        {
            try
            {
                var maps = _gameDataService.GetMaps();
                await HandleETagCaching(context, maps, data =>
                        GenerateHash(data));
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetMapPowerInfo(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object powerInfo = _gameDataService.GetMapPowerInfo(mapId);
                await HandleETagCaching(context, powerInfo, data =>
                        GenerateHash(data));
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetMapAnimals(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object animals = _gameDataService.GetMapAnimals(mapId);
                await HandleETagCaching(context, animals, data =>
                        GenerateHash(data));
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetMapThings(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object things = _gameDataService.GetMapThings(mapId);
                await HandleETagCaching(context, things, data =>
                        GenerateHash(data));
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetMapCreaturesSummary(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object creaturesSummary = _gameDataService.GetMapCreaturesSummary(mapId);
                HandleFiltering(context, ref creaturesSummary);
                await ResponseBuilder.Success(context.Response, creaturesSummary);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetFarmSummary(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object summary = _gameDataService.GenerateFarmSummary(mapId);
                HandleFiltering(context, ref summary);
                await ResponseBuilder.Success(context.Response, summary);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetWeather(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object weather = _gameDataService.GetWeather(mapId);
                HandleFiltering(context, ref weather);
                await ResponseBuilder.Success(context.Response, weather);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetGrowingZone(HttpListenerContext context)
        {
            try
            {
                string zoneIdStr = context.Request.QueryString["zone_id"];
                if (string.IsNullOrEmpty(zoneIdStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing zone_id parameter");
                }

                if (!int.TryParse(zoneIdStr, out int zoneId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid zone_id format");
                }

                var mapId = GetMapIdProperty(context);
                object zone = _gameDataService.GetGrowingZoneById(mapId, zoneId);
                if (zone == null)
                {
                    await ResponseBuilder.Error(
                        context.Response,
                        HttpStatusCode.NotFound,
                        $"Growing zone with id={zoneId} not found"
                    );
                    return;
                }

                HandleFiltering(context, ref zone);
                await ResponseBuilder.Success(context.Response, zone);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetZones(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object zones = _gameDataService.GetMapZones(mapId);
                HandleFiltering(context, ref zones);
                await ResponseBuilder.Success(context.Response, zones);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetBuildings(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object building = _gameDataService.GetMapBuildings(mapId);
                HandleFiltering(context, ref building);
                await ResponseBuilder.Success(context.Response, building);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}