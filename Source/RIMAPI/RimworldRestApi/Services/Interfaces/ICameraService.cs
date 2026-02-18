
using RIMAPI.CameraStreamer;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface ICameraService
    {
        ApiResult ChangeZoom(int zoom);
        ApiResult MoveToPosition(int x, int y);
        ApiResult StartStream(ICameraStream stream);
        ApiResult StopStream(ICameraStream stream);
        ApiResult SetupStream(ICameraStream stream, StreamConfigDto config);
        ApiResult<StreamStatusDto> GetStreamStatus(ICameraStream stream);
    }
}
