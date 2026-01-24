using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;
using RimWorld;
using Verse;

namespace RIMAPI.Controllers
{
    public class GameController
    {
        private readonly IGameStateService _gameStateService;
        private readonly RIMAPI_Settings _settings;
        private readonly ICachingService _cachingService;

        public GameController(
            IGameStateService gameStateService,
            RIMAPI_Settings settings,
            ICachingService cachingService
        )
        {
            _gameStateService = gameStateService;
            _settings = settings;
            _cachingService = cachingService;
        }


        [Get("/api/v1/version")]
        [EndpointMetadata("Get versions of: game, mod, API")]
        public async Task GetVersion(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/version",
                dataFactory: () => Task.FromResult(ApiResult<VersionDto>.Ok(
                    new VersionDto
                    {
                        RimWorldVersion = VersionControl.CurrentVersionString,
                        ModVersion = _settings.version,
                        ApiVersion = _settings.apiVersion,
                    }
                )),
                expiration: TimeSpan.FromMinutes(5),
                priority: CachePriority.Low,
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Get("/api/v1/game/state")]
        public async Task GetGameState(HttpListenerContext context)
        {
            var result = _gameStateService.GetGameState();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/mods/info")]
        [EndpointMetadata("Get list of active mods")]
        [ResponseExample(typeof(ApiResponse<List<ModInfoDto>>))]
        public async Task GetModsInfo(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/mods/info",
                dataFactory: () => Task.FromResult(_gameStateService.GetModsInfo()),
                expiration: TimeSpan.FromMinutes(1),
                priority: CachePriority.Normal,
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Post("/api/v1/game/select-area")]
        [EndpointMetadata("Select an area on the map")]
        public async Task SelectArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SelectAreaRequestDto>();
            var result = _gameStateService.SelectArea(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/select")]
        [EndpointMetadata("Select game object")]
        public async Task Select(HttpListenerContext context)
        {
            var objType = RequestParser.GetStringParameter(context, "type");
            var id = RequestParser.GetIntParameter(context, "id");
            var result = _gameStateService.Select(objType, id);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/deselect")]
        [EndpointMetadata("Clear game selection")]
        public async Task DeselectAll(HttpListenerContext context)
        {
            var result = _gameStateService.DeselectAll();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/open-tab")]
        [EndpointMetadata("Open interface tab")]
        public async Task OpenTab(HttpListenerContext context)
        {
            var tabName = RequestParser.GetStringParameter(context, "name");
            var result = _gameStateService.OpenTab(tabName);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/datetime")]
        [EndpointMetadata("Get in-game date and time")]
        public async Task GetCurrentMapDatetime(HttpListenerContext context)
        {
            var result = _gameStateService.GetCurrentMapDatetime();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/datetime/tile")]
        [EndpointMetadata("Get in-game date and time in global map tile")]
        public async Task GetWorldTileDatetime(HttpListenerContext context)
        {
            var tileId = RequestParser.GetIntParameter(context, "tile_id");
            var result = _gameStateService.GetWorldTileDatetime(tileId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/def/all")]
        [EndpointMetadata("Get in-game date and time in global map tile")]
        public async Task GetAllDefs(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AllDefsRequestDto>();

            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/def/all",
                dataFactory: () => Task.FromResult(_gameStateService.GetAllDefs(body)),
                expiration: TimeSpan.FromMinutes(5),
                priority: CachePriority.Normal,
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Post("/api/v1/game/send/letter")]
        public async Task PostLetter(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SendLetterRequestDto>();
            var result = _gameStateService.SendLetterSimple(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/speed")]
        public async Task SetGameSpeed(HttpListenerContext context)
        {
            var speed = RequestParser.GetIntParameter(context, "speed");
            var result = _gameStateService.SetGameSpeed(speed);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/save")]
        public async Task GameSave(HttpListenerContext context)
        {
            var saveName = RequestParser.GetStringParameter(context, "name");
            var result = _gameStateService.GameSave(saveName);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/load")]
        public async Task GameLoad(HttpListenerContext context)
        {
            var loadName = RequestParser.GetStringParameter(context, "name");
            var result = _gameStateService.GameLoad(loadName);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/start/devquick")]
        public async Task GameDevQuickStart(HttpListenerContext context)
        {
            var result = _gameStateService.GameDevQuickStart();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/start")]
        public async Task GameStart(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<NewGameStartRequestDto>();
            var result = _gameStateService.GameStart(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/game/settings")]
        public async Task GetGameSettings(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/game/settings",
                dataFactory: () => Task.FromResult(_gameStateService.GetCurrentSettings()),
                expiration: TimeSpan.FromSeconds(15),
                priority: CachePriority.Low,
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Post("/api/v1/game/settings/toggle/run-in-background")]
        public async Task ToggleRunInBackground(HttpListenerContext context)
        {
            var result = _gameStateService.ToggleRunInBackground();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/game/settings/run-in-background")]
        public async Task GetRunInBackground(HttpListenerContext context)
        {
            var result = _gameStateService.GetRunInBackground();
            await context.SendJsonResponse(result);
        }
    }
}
