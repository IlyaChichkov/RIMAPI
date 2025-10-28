using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Core;
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

        public async Task GetColonists(HttpListenerContext context)
        {
            try
            {
                object colonists = _gameDataService.GetColonists();
                HandleFiltering(context, ref colonists);
                await ResponseBuilder.Success(context.Response, colonists);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetColonist(HttpListenerContext context)
        {
            try
            {
                string idStr = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(idStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing colonist ID parameter");
                    return;
                }

                if (!int.TryParse(idStr, out int colonistId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid colonist ID format");
                    return;
                }

                object colonist = _gameDataService.GetColonist(colonistId);
                if (colonist == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist not found");
                    return;
                }
                HandleFiltering(context, ref colonist);
                await ResponseBuilder.Success(context.Response, colonist);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetColonistsDetailed(HttpListenerContext context)
        {
            try
            {
                object colonistsDetailed = _gameDataService.GetColonistsDetailed();
                HandleFiltering(context, ref colonistsDetailed);
                await ResponseBuilder.Success(context.Response, colonistsDetailed);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetColonistDetailed(HttpListenerContext context)
        {
            try
            {
                string idStr = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(idStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing colonist ID parameter");
                    return;
                }

                if (!int.TryParse(idStr, out int colonistId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid colonist ID format");
                    return;
                }

                object colonistDetailed = _gameDataService.GetColonistDetailed(colonistId);
                if (colonistDetailed == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist not found");
                    return;
                }
                HandleFiltering(context, ref colonistDetailed);
                await ResponseBuilder.Success(context.Response, colonistDetailed);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetColonistInventory(HttpListenerContext context)
        {
            try
            {
                string idStr = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(idStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing colonist ID parameter");
                    return;
                }

                if (!int.TryParse(idStr, out int colonistId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid colonist ID format");
                    return;
                }

                object colonistInventory = _gameDataService.GetColonistInventory(colonistId);
                if (colonistInventory == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist's inventory not found");
                    return;
                }
                HandleFiltering(context, ref colonistInventory);
                await ResponseBuilder.Success(context.Response, colonistInventory);
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

        public async Task GetColonistBody(HttpListenerContext context)
        {
            try
            {
                string idStr = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(idStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing colonist ID parameter");
                    return;
                }

                if (!int.TryParse(idStr, out int colonistId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid colonist ID format");
                    return;
                }

                object colonistBodyParts = _gameDataService.GetColonistBodyParts(colonistId);
                if (colonistBodyParts == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist's inventory not found");
                    return;
                }
                HandleFiltering(context, ref colonistBodyParts);
                await ResponseBuilder.Success(context.Response, colonistBodyParts);
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

        public async Task GetPawnOpinionAboutPawn(HttpListenerContext context)
        {
            try
            {
                string pawnIdStr = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(pawnIdStr))
                {
                    throw new Exception("Missing 'id' parameter");
                }

                if (!int.TryParse(pawnIdStr, out int pawnId))
                {
                    throw new Exception("Invalid 'id' format");
                }

                string otherIdStr = context.Request.QueryString["other_id"];
                if (string.IsNullOrEmpty(otherIdStr))
                {
                    throw new Exception("Missing 'id' parameter");
                }

                if (!int.TryParse(otherIdStr, out int otherId))
                {
                    throw new Exception("Invalid 'id' format");
                }

                object opinion = _gameDataService.GetOpinionAboutPawn(pawnId, otherId);
                await ResponseBuilder.Success(context.Response, opinion);
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