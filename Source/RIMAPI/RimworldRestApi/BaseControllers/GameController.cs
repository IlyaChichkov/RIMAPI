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
                object r = _gameDataService.GetModsInfo();
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
                object r = _gameDataService.GetModsInfo();
                HandleFiltering(context, ref r);
                await ResponseBuilder.Success(context.Response, r);
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
                object r = _gameDataService.GetColonists();
                HandleFiltering(context, ref r);
                await ResponseBuilder.Success(context.Response, r);
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
                string id = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(id))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing colonist ID parameter");
                    return;
                }

                if (!int.TryParse(id, out int colonistId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid colonist ID format");
                    return;
                }

                object r = _gameDataService.GetColonist(colonistId);
                if (r == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist not found");
                    return;
                }
                HandleFiltering(context, ref r);
                await ResponseBuilder.Success(context.Response, r);
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
                object r = _gameDataService.GetColonistsDetailed();

                await HandleETagCaching(context, r, data =>
                {
                    if (data.Count == 0) return "empty";

                    var maxId = data.Max(c => c.Id);
                    var totalHediffs = data.Sum(c => c.Hediffs?.Count ?? 0);
                    var maxHealth = data.Max(c => c.Health);

                    return GenerateHash(data.Count, maxId, totalHediffs, maxHealth);
                });
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
                var idStr = context.Request.QueryString["id"];
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

                object r = _gameDataService.GetColonistDetailed(colonistId);
                if (r == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist not found");
                    return;
                }
                await HandleETagCaching(context, r, data =>
                    GenerateHash(
                        data.Id,
                        data.Health,
                        data.Mood,
                        data.Hediffs?.Count ?? 0,
                        data.WorkPriorities?.Count ?? 0,
                        data.CurrentJob
                    )
                );
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
                string id = context.Request.QueryString["id"];
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

                object r = _gameDataService.GetColonistInventory(colonistId);
                if (r == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist's inventory not found");
                    return;
                }
                HandleFiltering(context, ref r);
                await ResponseBuilder.Success(context.Response, r);
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
                string name = context.Request.QueryString["name"];
                if (string.IsNullOrEmpty(name))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing item name parameter");
                    return;
                }

                var r = _gameDataService.GetItemImage(name);
                if (r == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Failed to get item's image");
                    return;
                }
                HandleFiltering(context, ref r);
                await ResponseBuilder.Success(context.Response, r);
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
                string id = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(id))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing colonist ID parameter");
                    return;
                }

                if (!int.TryParse(id, out int colonistId))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid colonist ID format");
                    return;
                }

                var r = _gameDataService.GetColonistBodyParts(colonistId);
                if (r == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist's inventory not found");
                    return;
                }
                HandleFiltering(context, ref r);
                await ResponseBuilder.Success(context.Response, r);
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
                var datetimeAtStr = context.Request.QueryString["at"];
                if (string.IsNullOrEmpty(datetimeAtStr))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing datetime 'at' parameter");
                    return;
                }

                if (datetimeAtStr == "current_map")
                {
                    var datetime = _gameDataService.GetCurrentMapDatetime();
                    await HandleETagCaching(context, datetime, data =>
                        GenerateHash(datetime)
                    );
                }
                else if (datetimeAtStr == "world_tile")
                {
                    var tileIdStr = context.Request.QueryString["tile_id"];

                    if (!int.TryParse(tileIdStr, out int tileId))
                    {
                        await ResponseBuilder.Error(context.Response,
                            HttpStatusCode.BadRequest, "Invalid tile ID format");
                        return;
                    }

                    object r = _gameDataService.GetWorldTileDatetime(tileId);
                    await HandleETagCaching(context, r, data =>
                        GenerateHash(data)
                    );
                }
                await ResponseBuilder.Error(
                    context.Response,
                    HttpStatusCode.BadRequest,
                    "Failed to parse 'at' parameter. Expected 'current_map' or 'world_tile'"
                );
                return;
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}