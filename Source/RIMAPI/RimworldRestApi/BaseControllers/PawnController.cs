using System;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PawnController : BaseController
    {
        private readonly IColonistService _colonistService;
        private readonly ICachingService _cachingService;

        public PawnController(IColonistService colonistService, ICachingService cachingService)
        {
            _colonistService = colonistService;
            _cachingService = cachingService;
        }

        [Get("/api/v1/colonists")]
        public async Task GetColonists(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/colonists",
                dataFactory: async () => _colonistService.GetColonists(),
                expiration: TimeSpan.FromSeconds(30),
                priority: CachePriority.Normal,
                expirationType: CacheExpirationType.Sliding
            );
        }

        [Get("/api/v1/colonist")]
        public async Task GetColonist(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "id");
            var result = _colonistService.GetColonist(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonists/detailed")]
        public async Task GetColonistsDetailed(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/colonists",
                dataFactory: async () => _colonistService.GetColonistsDetailed(),
                expiration: TimeSpan.FromSeconds(30),
                priority: CachePriority.Normal,
                expirationType: CacheExpirationType.Sliding
            );
        }

        [Get("/api/v1/colonist/detailed")]
        public async Task GetResearchProGetColonistDetailedgress(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "id");
            var result = _colonistService.GetColonistDetailed(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonist/opinion-about")]
        public async Task GetPawnOpinionAboutPawn(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "id");
            var otherId = RequestParser.GetIntParameter(context, "other_id");
            var result = _colonistService.GetOpinionAboutPawn(pawnId, otherId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonist/inventory")]
        public async Task GetColonistInventory(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "id");
            var result = _colonistService.GetColonistInventory(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/colonist/body/image")]
        public async Task GetPawnBodyImage(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "id");
            var result = _colonistService.GetColonistInventory(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawn/portrait/image")]
        public async Task GetPawnPortraitImage(HttpListenerContext context)
        {
            int pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            int width = RequestParser.GetIntParameter(context, "width");
            int height = RequestParser.GetIntParameter(context, "height");
            string direction = RequestParser.GetStringParameter(context, "direction");

            var result = _colonistService.GetPawnPortraitImage(pawnId, width, height, direction);

            await context.SendJsonResponse(result);
        }
    }
}
