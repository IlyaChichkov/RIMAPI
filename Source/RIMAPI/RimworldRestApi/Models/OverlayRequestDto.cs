namespace RIMAPI.Models
{
    public class OverlayRequestDto
    {
        public string Text { get; set; }
        public float Duration { get; set; } = 3.0f; // Seconds
        public string Color { get; set; } = "#FFFFFF"; // Hex code
        public float Scale { get; set; } = 2.0f; // Text size multiplier
    }
}