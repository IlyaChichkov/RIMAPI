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
        public float DaysToArrive { get; set; }
    }

    public class CaravanPathDto
    {
        public int Id { get; set; }
        public bool Moving { get; set; }
        public int CurrentTile { get; set; }
        public int NextTile { get; set; }
        public float Progress { get; set; }
        public int DestinationTile { get; set; }
        public List<int> Path { get; set; }
    }
}