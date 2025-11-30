using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PawnController : RequestParser
    {
        private readonly IColonistService _colonistService;

        public PawnController(IColonistService colonistService)
        {
            _colonistService = colonistService;
        }

        [Get("/api/v1/colonists")]
        public async Task GetColonists(HttpListenerContext context)
        {
            var result = _colonistService.GetColonists();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonist")]
        public async Task GetColonist(HttpListenerContext context)
        {
            var pawnId = GetIntParameter(context, "id");
            var result = _colonistService.GetColonist(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonists/detailed")]
        public async Task GetColonistsDetailed(HttpListenerContext context)
        {
            var result = _colonistService.GetColonistsDetailed();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonist/detailed")]
        public async Task GetResearchProGetColonistDetailedgress(HttpListenerContext context)
        {
            var pawnId = GetIntParameter(context, "id");
            var result = _colonistService.GetColonistDetailed(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonist/opinion-about")]
        public async Task GetPawnOpinionAboutPawn(HttpListenerContext context)
        {
            var pawnId = GetIntParameter(context, "id");
            var otherId = GetIntParameter(context, "other_id");
            var result = _colonistService.GetOpinionAboutPawn(pawnId, otherId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonist/inventory")]
        public async Task GetColonistInventory(HttpListenerContext context)
        {
            var pawnId = GetIntParameter(context, "id");
            var result = _colonistService.GetColonistInventory(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonist/body/image")]
        public async Task GetPawnBodyImage(HttpListenerContext context)
        {
            var pawnId = GetIntParameter(context, "id");
            var result = _colonistService.GetColonistInventory(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawn/portrait/image")]
        public async Task GetPawnPortraitImage(HttpListenerContext context)
        {
            int pawnId = GetIntParameter(context, "pawn_id");
            int width = GetIntParameter(context, "width");
            int height = GetIntParameter(context, "height");
            string direction = GetStringParameter(context, "direction");

            var result = _colonistService.GetPawnPortraitImage(pawnId, width, height, direction);

            await context.SendJsonResponse(result);
        }
    }
}
