using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class SpawnDropPodRequestDto
    {
        public int MapId { get; set; }
        public PositionDto Position { get; set; }
        public List<ThingDto> Items { get; set; }
        public string Faction { get; set; } // Optional: "PlayerColony", "Ancients", etc.
        public bool OpenDelay { get; set; } = true; // Delay opening slightly for effect
    }
}