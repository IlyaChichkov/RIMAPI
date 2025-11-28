using System;
using RIMAPI.CameraStreamer;
using RIMAPI.Core;
using RIMAPI.Models;
using Verse;

namespace RIMAPI.Services
{
    public class CameraService : ICameraService
    {
        public CameraService() { }

        public ApiResult ChangeZoom(int zoom)
        {
            try
            {
                Find.CameraDriver.SetRootPosAndSize(
                    Find.CameraDriver.MapPosition.ToVector3(),
                    zoom
                );
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }

        public ApiResult MoveToPosition(int x, int y)
        {
            try
            {
                IntVec3 position = new IntVec3(x, 0, y);
                Find.CameraDriver.JumpToCurrentMapLoc(position);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }

        public ApiResult<StreamStatusDto> GetStreamStatus(ICameraStream stream)
        {
            var result = stream.GetStatus();
            return ApiResult<StreamStatusDto>.Ok(result);
        }

        public ApiResult SetupStream(ICameraStream stream, StreamConfigDto config)
        {
            try
            {
                stream.SetConfig(config);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }

        public ApiResult StartStream(ICameraStream stream)
        {
            try
            {
                stream.StartStreaming();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }

        public ApiResult StopStream(ICameraStream stream)
        {
            try
            {
                stream.StopStreaming();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            return ApiResult.Ok();
        }
    }
}
