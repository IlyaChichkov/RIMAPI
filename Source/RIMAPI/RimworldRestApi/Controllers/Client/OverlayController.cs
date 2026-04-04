using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class OverlayController
    {
        private readonly IOverlayService _overlayService;

        public OverlayController(IOverlayService overlayService)
        {
            _overlayService = overlayService;
        }

        [Post("/api/v1/ui/announce")]
        public async Task ShowAnnouncement(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<OverlayRequestDto>();
            var result = _overlayService.ShowAnnouncement(body);
            await context.SendJsonResponse(result);
        }
    }
}