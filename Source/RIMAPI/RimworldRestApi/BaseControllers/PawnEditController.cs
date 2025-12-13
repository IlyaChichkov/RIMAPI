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

        [Post("/api/v1/pawn/edit")]
        public async Task EditPawn(HttpListenerContext context)
        {
            PawnEditRequestDto body;
            ApiResult result;
            try
            {
                body = await context.Request.ReadBodyAsync<PawnEditRequestDto>();
            }
            catch (Exception e)
            {
                result = ApiResult.Fail($"Failed to parse request body into Dto. Error: {e.Message}");
                await context.SendJsonResponse(result);
                return;
            }

            result = _pawnEditService.EditPawn(body);
            await context.SendJsonResponse(result);
        }
    }
}
