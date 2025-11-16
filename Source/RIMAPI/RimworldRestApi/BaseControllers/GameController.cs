using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Core;
using RimworldRestApi.Models;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class GameController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public GameController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task GetGameState(HttpListenerContext context)
        {
            try
            {
                var gameState = _gameDataService.GetGameState();
                await HandleETagCaching(context, gameState, data =>
                    GenerateHash(
                        data.GameTick,
                        data.ColonyWealth,
                        data.ColonistCount,
                        data.Storyteller
                    )
                );
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetModsInfo(HttpListenerContext context)
        {
            try
            {
                object mods = _gameDataService.GetModsInfo();
                HandleFiltering(context, ref mods);
                await ResponseBuilder.Success(context.Response, mods);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetItemImage(HttpListenerContext context)
        {
            try
            {
                string nameStr = context.Request.QueryString["name"];
                if (string.IsNullOrEmpty(nameStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing item name parameter");
                    return;
                }

                object itemImage = _gameDataService.GetItemImage(nameStr);
                if (itemImage == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Failed to get item's image");
                    return;
                }
                HandleFiltering(context, ref itemImage);
                await ResponseBuilder.Success(context.Response, itemImage);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetMapTime(HttpListenerContext context)
        {
            try
            {
                string datetimeAtStr = context.Request.QueryString["at"];
                if (string.IsNullOrEmpty(datetimeAtStr))
                {
                    throw new Exception("Missing at parameter");
                }

                if (datetimeAtStr == "current_map")
                {
                    object mapDatetime = _gameDataService.GetCurrentMapDatetime();
                    await ResponseBuilder.Success(context.Response, mapDatetime);
                }
                else if (datetimeAtStr == "world_tile")
                {
                    string tileIdStr = context.Request.QueryString["tile_id"];

                    if (!int.TryParse(tileIdStr, out int tileId))
                    {
                        throw new Exception("Invalid tile ID format");
                    }

                    object worldDatetime = _gameDataService.GetWorldTileDatetime(tileId);
                    await ResponseBuilder.Success(context.Response, worldDatetime);
                }
                else
                {
                    throw new Exception("Failed to parse 'at' parameter. Expected 'current_map' or 'world_tile'");
                }
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetQuestsData(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object questData = _gameDataService.GetQuestsData(mapId);
                HandleFiltering(context, ref questData);
                await ResponseBuilder.Success(context.Response, questData);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task DeselectGameObject(HttpListenerContext context)
        {
            try
            {
                var objType = GetStringProperty(context, "type");
                if (objType == "all")
                {
                    _gameDataService.DeselectAll();
                    var result = new
                    {
                        Result = "success"
                    };
                    await ResponseBuilder.Success(context.Response, result);
                }
                else
                {
                    // TODO: deselect by id
                }
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task SelectGameObject(HttpListenerContext context)
        {
            try
            {
                // TODO: select all
                var objType = GetStringProperty(context, "type");
                var id = GetIntProperty(context, "id");
                _gameDataService.SelectGameObject(objType, id);
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task OpenTab(HttpListenerContext context)
        {
            try
            {
                var tab = GetStringProperty(context, "type");
                _gameDataService.OpenTab(tab);
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetIncidentsData(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object incidentsData = _gameDataService.GetIncidentsData(mapId);
                HandleFiltering(context, ref incidentsData);
                await ResponseBuilder.Success(context.Response, incidentsData);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}