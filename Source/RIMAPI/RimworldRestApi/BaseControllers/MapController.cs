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
                var mapId = await GetMapIdProperty(context);
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
                var mapId = await GetMapIdProperty(context);
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
                var mapId = await GetMapIdProperty(context);
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
                var mapId = await GetMapIdProperty(context);
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
    }
}