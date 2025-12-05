using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.CameraStreamer;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class CameraController
    {
        private readonly ICameraStream _cameraStream;
        private readonly ICameraService _cameraService;

        public CameraController(ICameraService cameraService, ICameraStream cameraStream)
        {
            _cameraService = cameraService;
            _cameraStream = cameraStream;
        }

        [Post("/api/v1/camera/change/zoom")]
        [EndpointMetadata("Change game camera zoom")]
        public async Task ChangeZoom(HttpListenerContext context)
        {
            var zoom = RequestParser.GetIntParameter(context, "zoom");

            var result = _cameraService.ChangeZoom(zoom);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/camera/change/position")]
        [EndpointMetadata("Change game camera position")]
        public async Task MoveToPosition(HttpListenerContext context)
        {
            var x = RequestParser.GetIntParameter(context, "x");
            var y = RequestParser.GetIntParameter(context, "y");

            var result = _cameraService.MoveToPosition(x, y);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/stream/start")]
        [EndpointMetadata("Start game camera stream")]
        public async Task PostStreamStart(HttpListenerContext context)
        {
            var result = _cameraService.StartStream(_cameraStream);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/stream/stop")]
        [EndpointMetadata("Stop game camera stream")]
        public async Task PostStreamStop(HttpListenerContext context)
        {
            var result = _cameraService.StopStream(_cameraStream);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/stream/setup")]
        [EndpointMetadata("Set game camera stream configuration")]
        public async Task PostStreamSetup(HttpListenerContext context)
        {
            var requestData = await context.Request.ReadBodyAsync<StreamConfigDto>();
            var result = _cameraService.SetupStream(_cameraStream, requestData);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/stream/status")]
        [EndpointMetadata("Get game camera stream status")]
        public async Task GetStreamStatus(HttpListenerContext context)
        {
            var result = _cameraService.GetStreamStatus(_cameraStream);
            await context.SendJsonResponse(result);
        }
    }
}
