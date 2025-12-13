using RimWorld;
using Verse;

namespace RIMAPI.Models
{
    public class SiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Tile { get; set; }
        public FactionDto Faction { get; set; }
    }
}
