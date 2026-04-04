using Newtonsoft.Json;

namespace RIMAPI.Models.Camera
{
    public class CameraScreenshotRequestDto
    {
        [JsonProperty("format")]
        public string Format { get; set; } = "jpeg";

        [JsonProperty("quality")]
        public int Quality { get; set; } = 75;

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("hide_ui")]
        public bool HideUI { get; set; } = true;
    }


    public class NativeScreenshotRequestDto
    {
        public string FileName { get; set; } // Optional: Game auto-generates if empty
        public float? CenterX { get; set; }
        public float? CenterZ { get; set; }
        public float? ZoomLevel { get; set; } // Typically 10 to 60
        public bool HideUI { get; set; } = true;
    }

    public class CameraScreenshotResponseDto
    {
        public ImageData Image { get; set; }
        public ImageMetadata Metadata { get; set; }
        public GameContext GameContext { get; set; }
    }

    public class ImageData
    {
        public string DataUri { get; set; }
    }

    public class ImageMetadata
    {
        public string Format { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SizeBytes { get; set; }
    }

    public class GameContext
    {
        public int CurrentTick { get; set; }
    }
}
