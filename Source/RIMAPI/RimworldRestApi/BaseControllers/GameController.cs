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

    }
}