using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Services.Interfaces;
using RIMAPI.Services;
using RIMAPI.Models;

namespace RIMAPI.BaseControllers
{
    public class UIController
    {
        private readonly IUIService _uiService;
        private readonly IWindowService _windowService;

        public UIController(IUIService uiService, IWindowService windowService)
        {
            _uiService = uiService;
            _windowService = windowService;
        }

        [Post("/api/v1/ui/message")]
        public async Task ShowMessage(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<WindowMessageRequestDto>();
            var result = _windowService.ShowMessage(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/ui/dialog")]
        public async Task ShowDialog(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<WindowDialogRequestDto>();
            var result = _windowService.ShowDialog(body);
            await context.SendJsonResponse(result);
        }
    }
}