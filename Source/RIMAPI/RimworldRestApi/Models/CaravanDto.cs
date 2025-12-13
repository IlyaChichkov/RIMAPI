using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class CaravanDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPlayerControlled { get; set; }
        public PositionDto Position { get; set; }
        public int Tile { get; set; }
        public List<PawnDto> Pawns { get; set; }
        public List<ThingDto> Items { get; set; }
        public float MassUsage { get; set; }
        public float MassCapacity { get; set; }
        public string Forageability { get; set; }
        public string Visibility { get; set; }
    }

    public class PawnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}