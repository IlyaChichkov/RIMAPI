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

        public async Task GetModsInfo(HttpListenerContext context)
        {
            var modsInfo = _gameDataService.GetModsInfo();

            await HandleETagCaching(context, modsInfo, data =>
                GenerateHash(data)
            );
        }

        public async Task GetColonists(HttpListenerContext context)
        {
            try
            {
                var colonists = _gameDataService.GetColonists();
                await ResponseBuilder.Success(context.Response, colonists);
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting colonists - {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Error retrieving colonists");
            }
        }

        public async Task GetColonist(HttpListenerContext context)
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

                var colonist = _gameDataService.GetColonist(colonistId);
                if (colonist == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist not found");
                    return;
                }

                await ResponseBuilder.Success(context.Response, colonist);
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting colonist - {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Error retrieving colonist");
            }
        }

        public async Task GetColonistsDetailed(HttpListenerContext context)
        {
            try
            {
                var colonists = _gameDataService.GetColonistsDetailed();

                // Use more comprehensive hash for detailed data
                await HandleETagCaching(context, colonists, data =>
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
                Log.Error($"RIMAPI: Error getting detailed colonists - {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Error retrieving detailed colonists");
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

                var colonist = _gameDataService.GetColonistDetailed(colonistId);
                if (colonist == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist not found");
                    return;
                }

                // Comprehensive hash for detailed colonist data
                await HandleETagCaching(context, colonist, data =>
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
                Log.Error($"RIMAPI: Error getting detailed colonist - {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Error retrieving detailed colonist");
            }
        }

        public async Task GetColonistInventory(HttpListenerContext context)
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

                var colonistInventory = _gameDataService.GetColonistInventory(colonistId);
                if (colonistInventory == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist's inventory not found");
                    return;
                }
                await ResponseBuilder.Success(context.Response, colonistInventory);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, $"Error retrieving data - {ex}");
            }
        }

        public async Task GetItemImage(HttpListenerContext context)
        {
            try
            {
                var name = context.Request.QueryString["name"];
                if (string.IsNullOrEmpty(name))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Missing item name parameter");
                    return;
                }

                var image = _gameDataService.GetItemImage(name);
                if (image == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Failed to get item's image");
                    return;
                }
                await ResponseBuilder.Success(context.Response, image);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, $"Error retrieving data - {ex}");
            }
        }

        public async Task GetColonistBody(HttpListenerContext context)
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

                var colonistInventory = _gameDataService.GetColonistBodyParts(colonistId);
                if (colonistInventory == null)
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.NotFound, "Colonist's inventory not found");
                    return;
                }
                await ResponseBuilder.Success(context.Response, colonistInventory);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, $"Error retrieving data - {ex}");
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

                    var datetime = _gameDataService.GetWorldTileDatetime(tileId);
                    await HandleETagCaching(context, datetime, data =>
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
                    HttpStatusCode.InternalServerError, $"Error retrieving data - {ex}");
            }
        }
    }
}