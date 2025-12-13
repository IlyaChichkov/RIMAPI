using System.Collections.Generic;

namespace RIMAPI.Models
{
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
    }
}
