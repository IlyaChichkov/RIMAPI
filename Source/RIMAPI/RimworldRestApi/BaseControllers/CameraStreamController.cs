using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.CameraStreamer;
using RimworldRestApi.Core;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class CameraController : BaseController
    {
        private readonly IGameDataService _gameDataService;
        private UdpCameraStreamer _udpStreamer;

        public CameraController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
            _udpStreamer = new UdpCameraStreamer();
        }

        public async Task ChangeZoom(HttpListenerContext context)
        {
            try
            {
                var zoom = GetIntProperty(context, "zoom");

                Find.CameraDriver.SetRootPosAndSize(Find.CameraDriver.MapPosition.ToVector3(), zoom);
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task MoveToPosition(HttpListenerContext context)
        {
            try
            {
                var x = GetIntProperty(context, "x");
                var y = GetIntProperty(context, "y");

                IntVec3 position = new IntVec3(x, 0, y);
                Find.CameraDriver.JumpToCurrentMapLoc(position);
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task PostStreamStart(HttpListenerContext context)
        {
            try
            {
                _udpStreamer.StartStreaming();
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task PostStreamStop(HttpListenerContext context)
        {
            try
            {
                _udpStreamer.StopStreaming();
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task PostStreamSetup(HttpListenerContext context)
        {
            try
            {
                if (_udpStreamer.IsStreaming)
                {
                    throw new Exception("Failed to setup camera streamer during streaming process, stop it before loading new config");
                }

                string addressStr = context.Request.QueryString["ip"];
                if (string.IsNullOrEmpty(addressStr))
                {
                    throw new Exception("Missing 'ip' parameter");
                }

                string portStr = context.Request.QueryString["port"];
                if (string.IsNullOrEmpty(portStr))
                {
                    throw new Exception("Missing 'port' parameter");
                }
                if (!int.TryParse(portStr, out int port))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid 'port' format");
                    return;
                }

                string frameWidthStr = context.Request.QueryString["frame_width"];
                if (string.IsNullOrEmpty(portStr))
                {
                    throw new Exception("Missing 'frame_width' parameter");
                }
                if (!int.TryParse(frameWidthStr, out int frameWidth))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid 'frame_width' format");
                    return;
                }

                string frameHeightStr = context.Request.QueryString["frame_height"];
                if (string.IsNullOrEmpty(portStr))
                {
                    throw new Exception("Missing 'frame_height' parameter");
                }
                if (!int.TryParse(frameHeightStr, out int frameHeight))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid 'frame_height' format");
                    return;
                }

                string targetFPSStr = context.Request.QueryString["fps"];
                if (string.IsNullOrEmpty(portStr))
                {
                    throw new Exception("Missing 'fps' parameter");
                }
                if (!int.TryParse(targetFPSStr, out int fps))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid 'fps' format");
                    return;
                }

                string jpegQualityStr = context.Request.QueryString["quality"];
                if (string.IsNullOrEmpty(portStr))
                {
                    throw new Exception("Missing 'quality' parameter");
                }
                if (!int.TryParse(jpegQualityStr, out int quality))
                {
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.BadRequest, "Invalid 'quality' format");
                    return;
                }

                _udpStreamer.Setup(new StreamingSetup
                {
                    Port = port,
                    Address = addressStr,
                    FrameWidth = frameWidth,
                    FrameHeight = frameHeight,
                    TargetFPS = fps,
                    JpegQuality = quality,
                });
                var result = new
                {
                    Result = "success",
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetStreamStatus(HttpListenerContext context)
        {
            try
            {
                var setup = _udpStreamer.GetCurrentSetup();
                var result = new
                {
                    IsStreaming = _udpStreamer.IsStreaming,
                    Setup = setup,
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}