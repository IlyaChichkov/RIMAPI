using System;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PawnEditController
    {
        private readonly IPawnEditService _pawnEditService;

        public PawnEditController(IPawnEditService pawnEditService)
        {
            _pawnEditService = pawnEditService;
        }

        [Post("/api/v1/pawn/edit/basic")]
        public async Task EditBasicInfo(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnBasicRequest>();
            var result = _pawnEditService.UpdateBasicInfo(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/health")]
        public async Task EditHealth(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnHealthRequest>();
            var result = _pawnEditService.UpdateHealth(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/needs")]
        public async Task EditNeeds(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnNeedsRequest>();
            var result = _pawnEditService.UpdateNeeds(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/skills")]
        public async Task EditSkills(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnSkillsRequest>();
            var result = _pawnEditService.UpdateSkills(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/traits")]
        public async Task EditTraits(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnTraitsRequest>();
            var result = _pawnEditService.UpdateTraits(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/inventory")]
        public async Task EditInventory(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnInventoryRequest>();
            var result = _pawnEditService.UpdateInventory(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/apparel")]
        public async Task EditApparel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnApparelRequest>();
            var result = _pawnEditService.UpdateApparel(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/status")]
        public async Task EditStatus(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnStatusRequest>();
            var result = _pawnEditService.UpdateStatus(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/position")]
        public async Task EditPosition(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnPositionRequest>();
            var result = _pawnEditService.UpdatePosition(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/faction")]
        public async Task EditFaction(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnFactionRequest>();
            var result = _pawnEditService.UpdateFaction(body);
            await context.SendJsonResponse(result);
        }
    }
}