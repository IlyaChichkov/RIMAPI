using System.Net;
using System.Threading.Tasks;
using RIMAPI.CameraStreamer;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Models.Camera;
using RIMAPI.Services;

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
        public async Task ChangeZoom(RimApiContext context)
        {
            var zoom = RequestParser.GetIntParameter(context, "zoom");

            var result = _cameraService.ChangeZoom(zoom);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/camera/change/position")]
        [EndpointMetadata("Change game camera position")]
        public async Task MoveToPosition(RimApiContext context)
        {
            var x = RequestParser.GetIntParameter(context, "x");
            var y = RequestParser.GetIntParameter(context, "y");

            var result = _cameraService.MoveToPosition(x, y);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/camera/follow/pawn")]
        [EndpointMetadata("Jump/follow the camera to a pawn by id (cinematic capture).")]
        public async Task JumpToPawn(RimApiContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");

            var result = _cameraService.JumpToPawn(pawnId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/camera/screenshot")]
        [EndpointMetadata("Capture a screenshot of the game view. Supports resizing and format changes.")]
        public async Task MakeScreenshot(RimApiContext context)
        {
            var request = await context.Request.ReadBodyAsync<CameraScreenshotRequestDto>();
            var result = await _cameraService.MakeScreenshotAsync(request);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/camera/screenshot/native")]
        [EndpointMetadata("Moves the camera and saves a high-quality screenshot directly to the host machine's hard drive.")]
        public async Task TriggerNativeScreenshot(RimApiContext context)
        {
            var request = await context.Request.ReadBodyAsync<NativeScreenshotRequestDto>();
            var result = _cameraService.TakeNativeScreenshot(request);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/camera/stream/start")]
        [EndpointMetadata("Start game camera stream")]
        public async Task PostStreamStart(RimApiContext context)
        {
            var result = _cameraService.StartStream(_cameraStream);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/camera/stream/stop")]
        [EndpointMetadata("Stop game camera stream")]
        public async Task PostStreamStop(RimApiContext context)
        {
            var result = _cameraService.StopStream(_cameraStream);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/camera/stream/setup")]
        [EndpointMetadata("Set game camera stream configuration")]
        public async Task PostStreamSetup(RimApiContext context)
        {
            var requestData = await context.Request.ReadBodyAsync<StreamConfigDto>();
            var result = _cameraService.SetupStream(_cameraStream, requestData);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/camera/stream/status")]
        [EndpointMetadata("Get game camera stream status")]
        public async Task GetStreamStatus(RimApiContext context)
        {
            var result = _cameraService.GetStreamStatus(_cameraStream);
            await context.SendJsonResponse(result);
        }
    }
}
