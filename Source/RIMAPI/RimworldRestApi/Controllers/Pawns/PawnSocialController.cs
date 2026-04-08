using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PawnSocialController
    {
        private readonly IPawnSocialService _socialService;

        public PawnSocialController(IPawnSocialService socialService)
        {
            _socialService = socialService;
        }

        [Get("/api/v1/game/defs/interactions")]
        [EndpointMetadata("Retrieves a list of all valid InteractionDef names in the game.")]
        public async Task GetInteractionDefs(HttpListenerContext context)
        {
            var result = _socialService.GetInteractionDefs();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawns/interactions")]
        [EndpointMetadata("Gets the real-time interaction readiness of a specific pawn.")]
        public async Task GetPawnInteractionStatus(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _socialService.GetPawnInteractionStatus(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawns/interactions/log")]
        [EndpointMetadata("Retrieves the recent interaction history for a specific pawn.")]
        public async Task GetPawnInteractionLog(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _socialService.GetPawnInteractionLog(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawns/relations")]
        [EndpointMetadata("Retrieves all established familial and romantic relationships for this pawn.")]
        public async Task GetPawnRelations(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _socialService.GetPawnRelations(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawns/opinions")]
        [EndpointMetadata("Retrieves a list of this pawn's numerical opinion toward every other colonist on the map.")]
        public async Task GetPawnOpinions(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _socialService.GetPawnOpinions(pawnId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawns/interactions/force")]
        [EndpointMetadata("Forces a specific social interaction to occur immediately between two pawns.")]
        public async Task ForceInteraction(HttpListenerContext context)
        {
            var request = await context.Request.ReadBodyAsync<ForceInteractionRequestDto>();
            var result = _socialService.ForceInteraction(request);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawns/relations/add")]
        [EndpointMetadata("Instantly creates a permanent social bond between two pawns.")]
        public async Task AddRelation(HttpListenerContext context)
        {
            var request = await context.Request.ReadBodyAsync<AddRelationRequestDto>();
            var result = _socialService.AddRelation(request);
            await context.SendJsonResponse(result);
        }

        [Delete("/api/v1/pawns/relations/remove")]
        [EndpointMetadata("Severs a specific social bond between two pawns.")]
        public async Task RemoveRelation(HttpListenerContext context)
        {
            var request = await context.Request.ReadBodyAsync<RemoveRelationRequestDto>();
            var result = _socialService.RemoveRelation(request);
            await context.SendJsonResponse(result);
        }
    }
}
