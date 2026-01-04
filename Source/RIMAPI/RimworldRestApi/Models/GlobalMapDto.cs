using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RIMAPI.Models
{
    public class SettlementDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TileId { get; set; }
        public FactionDto Faction { get; set; }
    }

    public class SiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TileId { get; set; }
        public FactionDto Faction { get; set; }
    }

    public class TileDto
    {
        public int Id { get; set; }
        public string Biome { get; set; }
        public float Elevation { get; set; }
        public float Temperature { get; set; }
        public float Rainfall { get; set; }
        public string Hilliness { get; set; }
        public List<string> Roads { get; set; }
        public List<string> Rivers { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public bool IsPolluted { get; set; }
        public float Pollution { get; set; }
    }

    public class CoordinatesDto
    {
        public float Lat { get; set; }
        public float Lon { get; set; }
    }
}
