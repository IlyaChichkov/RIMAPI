using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class FactionController
    {
        private readonly IFactionService _factionService;

        public FactionController(IFactionService factionService)
        {
            _factionService = factionService;
        }

        [Get("/api/v1/factions")]
        public async Task GetFactions(HttpListenerContext context)
        {
            var result = _factionService.GetFactions();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/faction/player")]
        public async Task GetPlayerFaction(HttpListenerContext context)
        {
            var result = _factionService.GetPlayerFaction();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/faction")]
        public async Task GetFaction(HttpListenerContext context)
        {
            int id = RequestParser.GetIntParameter(context, "id");
            var result = _factionService.GetFaction(id);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/faction/relation-with")]
        public async Task GetFactionRelationWith(HttpListenerContext context)
        {
            int id = RequestParser.GetIntParameter(context, "id");
            int otherId = RequestParser.GetIntParameter(context, "other_id");
            var result = _factionService.GetFactionRelationWith(id, otherId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/faction/relations")]
        public async Task GetFactionRelations(HttpListenerContext context)
        {
            int id = RequestParser.GetIntParameter(context, "id");
            var result = _factionService.GetFactionRelations(id);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/faction/def")]
        public async Task GetFactionDef(HttpListenerContext context)
        {
            var name = RequestParser.GetStringParameter(context, "name");
            var result = _factionService.GetFactionDef(name);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/faction/change/goodwill")]
        public async Task ChangeFactionRelationWith(HttpListenerContext context)
        {
            FactionChangeRelationRequestDto body =
                await context.Request.ReadBodyAsync<FactionChangeRelationRequestDto>();
            var result = _factionService.ChangeFactionRelationWith(
                body.Id,
                body.OtherId,
                body.Value,
                body.SendMessage,
                body.CanSendHostilityLetter
            );
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/faction/goodwill")]
        public async Task SetFactionGoodwill(HttpListenerContext context)
        {
            FactionChangeRelationRequestDto body =
                await context.Request.ReadBodyAsync<FactionChangeRelationRequestDto>();
            var result = _factionService.SetFactionGoodwill(
                body.Id,
                body.OtherId,
                body.Value,
                body.SendMessage,
                body.CanSendHostilityLetter
            );
            await context.SendJsonResponse(result);
        }
    }
}
