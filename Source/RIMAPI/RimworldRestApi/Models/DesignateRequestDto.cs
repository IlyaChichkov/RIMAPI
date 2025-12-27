using System;
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class DesignateRequestDto
    {
        public int MapId { get; set; }
        public string Type { get; set; } // "Mine", "Deconstruct", "Harvest", "Hunt"
        public PositionDto PointA { get; set; }
        public PositionDto PointB { get; set; }
    }
}
