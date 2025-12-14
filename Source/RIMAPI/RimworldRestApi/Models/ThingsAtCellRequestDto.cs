using RIMAPI.Models;

namespace RIMAPI.Models
{
    public class ThingsAtCellRequestDto
    {
        public int MapId { get; set; }
        public PositionDto Position { get; set; }
    }
}
