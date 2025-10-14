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
                Log.Error($"RIMAPI: Error getting colonists - {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Error retrieving colonists");
            }
        }

        public async Task GetMapPowerInfo(HttpListenerContext context)
        {
            try
            {
                var mapIdStr = context.Request.QueryString["mapId"];
                if (string.IsNullOrEmpty(mapIdStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing mapId parameter");
                    return;
                }

                if (!int.TryParse(mapIdStr, out int mapId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid mapId format");
                    return;
                }

                object powerInfo = _gameDataService.GetMapPowerInfo(mapId);
                await HandleETagCaching(context, powerInfo, data =>
                        GenerateHash(data));
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting map power info: {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Error retrieving map power info");
            }
        }

        public async Task GetMapAnimals(HttpListenerContext context)
        {
            try
            {
                var mapIdStr = context.Request.QueryString["mapId"];
                if (string.IsNullOrEmpty(mapIdStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing mapId parameter");
                    return;
                }

                if (!int.TryParse(mapIdStr, out int mapId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid mapId format");
                    return;
                }

                object animals = _gameDataService.GetMapAnimals(mapId);
                await HandleETagCaching(context, animals, data =>
                        GenerateHash(data));
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting map animals: {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Error retrieving map animals");
            }
        }

        public async Task GetMapThings(HttpListenerContext context)
        {
            try
            {
                string mapIdStr = context.Request.QueryString["mapId"];
                if (string.IsNullOrEmpty(mapIdStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing mapId parameter");
                    return;
                }

                if (!int.TryParse(mapIdStr, out int mapId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid mapId format");
                    return;
                }

                object things = _gameDataService.GetMapThings(mapId);
                await HandleETagCaching(context, things, data =>
                        GenerateHash(data));
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting map things: {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Error retrieving map things");
            }
        }
    }
}