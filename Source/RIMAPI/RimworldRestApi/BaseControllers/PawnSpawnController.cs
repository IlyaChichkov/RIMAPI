using System;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PawnSpawnController
    {
        private readonly IPawnSpawnService _pawnSpawnService;

        public PawnSpawnController(IPawnSpawnService pawnSpawnService)
        {
            _pawnSpawnService = pawnSpawnService;
        }

        [Post("/api/v1/pawn/spawn")]
        public async Task EditPawn(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnSpawnRequestDto>();
            var result = _pawnSpawnService.SpawnPawn(body);
            await context.SendJsonResponse(result);
        }
    }
}
