using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class DevToolsController : RequestParser
    {
        private readonly IDevToolsService _devToolsService;

        public DevToolsController(IDevToolsService gameStateService)
        {
            _devToolsService = gameStateService;
        }

        [Post("/api/v1/dev/console")]
        [EndpointDescription("Send message to the debug console")]
        public async Task PostConsoleAction(HttpListenerContext context)
        {
            var action = GetStringParameter(context, "action");
            var result = _devToolsService.ConsoleAction(action);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/dev/materials-atlas")]
        [ResponseExample(typeof(MaterialsAtlasList))]
        public async Task GetMaterialsAtlasList(HttpListenerContext context)
        {
            var result = _devToolsService.GetMaterialsAtlasList();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/dev/materials-atlas/clear")]
        public async Task PostMaterialsAtlasPoolClear(HttpListenerContext context)
        {
            var result = _devToolsService.MaterialsAtlasPoolClear();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/dev/stuff/color")]
        [EndpointDescription("Change stuff color")]
        public async Task PostStuffColor(HttpListenerContext context)
        {
            var requestData = await context.Request.ReadBodyAsync<StuffColorRequest>();
            var result = _devToolsService.SetStuffColor(requestData);
            await context.SendJsonResponse(result);
        }
    }
}
