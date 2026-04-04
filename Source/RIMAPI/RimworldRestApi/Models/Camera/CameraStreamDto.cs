namespace RIMAPI.Models
{
    public class StreamStatusDto
    {
        public bool IsStreaming { get; set; }
        public StreamConfigDto Config { get; set; }
    }

    public class StreamConfigDto
    {
        public int Port { get; set; }
        public string Address { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public int TargetFps { get; set; }
        public int JpegQuality { get; set; }
    }
}
