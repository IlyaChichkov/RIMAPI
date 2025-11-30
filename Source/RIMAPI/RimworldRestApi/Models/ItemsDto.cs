using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class ThingDto
    {
        public int ThingId { get; set; }
        public string DefName { get; set; }
        public string Label { get; set; }
        public List<string> Categories { get; set; }
        public PositionDto Position { get; set; }
        public int StackCount { get; set; }
        public double MarketValue { get; set; }
        public bool IsForbidden { get; set; }
        public int Quality { get; set; }
        public int HitPoints { get; set; }
        public int MaxHitPoints { get; set; }
    }
}
