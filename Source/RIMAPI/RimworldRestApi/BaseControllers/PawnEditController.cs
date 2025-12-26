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

        /// <summary>
        /// Helper wrapper to handle deserialization and error reporting uniformly.
        /// </summary>
        private async Task HandleRequest<T>(HttpListenerContext context, Func<T, ApiResult> serviceAction) where T : class
        {
            T body;
            try
            {
                body = await context.Request.ReadBodyAsync<T>();
            }
            catch (Exception e)
            {
                await context.SendJsonResponse(ApiResult.Fail($"Failed to parse request body for {typeof(T).Name}. Error: {e.Message}"));
                return;
            }

            if (body == null)
            {
                await context.SendJsonResponse(ApiResult.Fail("Request body was empty or invalid."));
                return;
            }

            // Execute service method
            var result = serviceAction(body);
            await context.SendJsonResponse(result);
        }

        // --- Endpoints ---

        [Post("/api/v1/pawn/edit/basic")]
        public async Task EditBasicInfo(HttpListenerContext context)
        {
            await HandleRequest<PawnBasicRequest>(context, _pawnEditService.UpdateBasicInfo);
        }

        [Post("/api/v1/pawn/edit/health")]
        public async Task EditHealth(HttpListenerContext context)
        {
            await HandleRequest<PawnHealthRequest>(context, _pawnEditService.UpdateHealth);
        }

        [Post("/api/v1/pawn/edit/needs")]
        public async Task EditNeeds(HttpListenerContext context)
        {
            await HandleRequest<PawnNeedsRequest>(context, _pawnEditService.UpdateNeeds);
        }

        [Post("/api/v1/pawn/edit/skills")]
        public async Task EditSkills(HttpListenerContext context)
        {
            await HandleRequest<PawnSkillsRequest>(context, _pawnEditService.UpdateSkills);
        }

        [Post("/api/v1/pawn/edit/traits")]
        public async Task EditTraits(HttpListenerContext context)
        {
            await HandleRequest<PawnTraitsRequest>(context, _pawnEditService.UpdateTraits);
        }

        [Post("/api/v1/pawn/edit/inventory")]
        public async Task EditInventory(HttpListenerContext context)
        {
            await HandleRequest<PawnInventoryRequest>(context, _pawnEditService.UpdateInventory);
        }

        [Post("/api/v1/pawn/edit/apparel")]
        public async Task EditApparel(HttpListenerContext context)
        {
            await HandleRequest<PawnApparelRequest>(context, _pawnEditService.UpdateApparel);
        }

        [Post("/api/v1/pawn/edit/status")]
        public async Task EditStatus(HttpListenerContext context)
        {
            await HandleRequest<PawnStatusRequest>(context, _pawnEditService.UpdateStatus);
        }

        [Post("/api/v1/pawn/edit/position")]
        public async Task EditPosition(HttpListenerContext context)
        {
            await HandleRequest<PawnPositionRequest>(context, _pawnEditService.UpdatePosition);
        }

        [Post("/api/v1/pawn/edit/faction")]
        public async Task EditFaction(HttpListenerContext context)
        {
            await HandleRequest<PawnFactionRequest>(context, _pawnEditService.UpdateFaction);
        }
    }
}