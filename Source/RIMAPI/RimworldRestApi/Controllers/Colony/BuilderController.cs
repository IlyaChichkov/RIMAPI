using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;
using RIMAPI.Http;

namespace RIMAPI.Controllers
{
    public class BuilderController
    {
        private readonly IBuilderService _builderService;

        public BuilderController(IBuilderService builderService)
        {
            _builderService = builderService;
        }

        [Post("/api/v1/builder/copy")]
        public async Task CopyArea(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<CopyAreaRequestDto>();
            var result = _builderService.CopyArea(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/builder/paste")]
        public async Task PasteArea(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<PasteAreaRequestDto>();
            var result = _builderService.PasteArea(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/builder/blueprint")]
        public async Task PlaceBlueprints(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<PasteAreaRequestDto>();
            var result = _builderService.PlaceBlueprints(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/builder/check-zone")]
        public async Task CheckZone(RimApiContext context)
        {
            var body = await context.Request.ReadBodyAsync<CheckZoneRequestDto>();
            var result = _builderService.CheckZone(body);
            await context.SendJsonResponse(result);
        }
    }
}