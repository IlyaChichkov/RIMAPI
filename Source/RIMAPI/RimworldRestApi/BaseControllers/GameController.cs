using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;
using RimWorld;

namespace RIMAPI.Controllers
{
    public class GameController : RequestParser
    {
        private readonly IGameStateService _gameStateService;
        private readonly RIMAPI_Settings _settings;

        public GameController(IGameStateService gameStateService, RIMAPI_Settings settings)
        {
            _gameStateService = gameStateService;
            _settings = settings;
        }

        [Get("/api/v1/version")]
        [EndpointDescription("Get versions of: game, mod, API")]
        public async Task GetVersion(HttpListenerContext context)
        {
            ApiResult<VersionDto> version = ApiResult<VersionDto>.Ok(
                new VersionDto
                {
                    RimWorldVersion = VersionControl.CurrentVersionString,
                    ModVersion = _settings.version,
                    ApiVersion = _settings.apiVersion,
                }
            );
            await context.SendJsonResponse(version);
        }

        [Get("/api/v1/game/state")]
        public async Task GetGameState(HttpListenerContext context)
        {
            var result = _gameStateService.GetGameState();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/mods/info")]
        [EndpointDescription("Get list of active mods")]
        [ResponseExample(typeof(ApiResponse<List<ModInfoDto>>))]
        public async Task GetModsInfo(HttpListenerContext context)
        {
            var result = _gameStateService.GetModsInfo();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/deselect")]
        [EndpointDescription("Clear game selection")]
        public async Task DeselectAll(HttpListenerContext context)
        {
            var result = _gameStateService.DeselectAll();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/open-tab")]
        [EndpointDescription("Open interface tab")]
        public async Task OpenTab(HttpListenerContext context)
        {
            var tabName = GetStringParameter(context, "name");
            var result = _gameStateService.OpenTab(tabName);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/datetime")]
        public async Task GetCurrentMapDatetime(HttpListenerContext context)
        {
            var result = _gameStateService.GetCurrentMapDatetime();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/datetime/tile")]
        public async Task GetWorldTileDatetime(HttpListenerContext context)
        {
            var tileId = GetIntParameter(context, "tile_id");
            var result = _gameStateService.GetWorldTileDatetime(tileId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/def/all")]
        public async Task GetAllDefs(HttpListenerContext context)
        {
            var result = _gameStateService.GetAllDefs();
            await context.SendJsonResponse(result);
        }
    }
}
