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
                var colonists = _gameDataService.GetMaps();
                await ResponseBuilder.Success(context.Response, colonists);
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

                var powerInfo = _gameDataService.GetMapPowerInfo(mapId);
                await ResponseBuilder.Success(context.Response, powerInfo);
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting map power info: {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Error retrieving map power info");
            }
        }
    }
}