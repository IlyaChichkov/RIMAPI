using System;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
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

        [Get("/api/v1/trait-def")]
        public async Task GetTraitDef(HttpListenerContext context)
        {
            string name = RequestParser.GetStringParameter(context, "name");
            var result = _colonistService.GetTraitDefDto(name);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/time-assignments")]
        public async Task GetTimeAssignmentsList(HttpListenerContext context)
        {
            var result = _colonistService.GetTimeAssignmentsList();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/colonist/time-assignment")]
        [EndpointMetadata("Schedule pawn assignment during provided hour")]
        public async Task SetTimeAssignment(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var hour = RequestParser.GetIntParameter(context, "hour");
            var assignment = RequestParser.GetStringParameter(context, "assignment");
            var result = _colonistService.SetTimeAssignment(pawnId, hour, assignment);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/outfits")]
        public async Task GetOutfits(HttpListenerContext context)
        {
            var result = _colonistService.GetOutfits();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/work-list")]
        public async Task GetWorkList(HttpListenerContext context)
        {
            var result = _colonistService.GetWorkList();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/colonist/work-priority")]
        [EndpointMetadata("Set pawn work priorities")]
        public async Task SetColonistWorkPriority(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "id");
            var workDef = RequestParser.GetStringParameter(context, "work");
            var priority = RequestParser.GetIntParameter(context, "priority");
            var result = _colonistService.SetColonistWorkPriority(pawnId, workDef, priority);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/colonists/work-priority")]
        [EndpointMetadata("Set multiple work priorities to several pawns")]
        public async Task SetColonistsWorkPriority(HttpListenerContext context)
        {
            var postData = await context.Request.ReadBodyAsync<ColonistsWorkPrioritiesRequestDto>();
            var result = _colonistService.SetColonistsWorkPriority(postData);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/jobs/make/equip")]
        [EndpointMetadata("Set multiple work priorities to several pawns")]
        public async Task MakeJobEquip(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var itemId = RequestParser.GetIntParameter(context, "item_id");
            var itemType = RequestParser.GetStringParameter(context, "item_type");
            var result = _colonistService.MakeJobEquip(mapId, pawnId, itemId, itemType);
            await context.SendJsonResponse(result);
        }
    }
}
