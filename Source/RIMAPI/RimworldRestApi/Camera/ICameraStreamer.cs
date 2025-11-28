using RIMAPI.Models;

namespace RIMAPI.CameraStreamer
{
    public interface ICameraStream
    {
        void SetConfig(StreamConfigDto setup);
        StreamConfigDto GetConfig();
        StreamStatusDto GetStatus();
        void StartStreaming();
        void StopStreaming();
        void Update();
        void SetResolution(int width, int height);
    }
}
