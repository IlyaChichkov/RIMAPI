using RIMAPI.Models;

namespace RIMAPI.Models
{
    public class SelectAreaRequestDto
    {
        public PositionDto PositionA { get; set; }
        public PositionDto PositionB { get; set; }
    }
}
