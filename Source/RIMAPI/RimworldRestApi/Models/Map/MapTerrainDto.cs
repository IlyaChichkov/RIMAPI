using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class MapTerrainDto
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<string> Palette { get; set; }  // Natural terrain types (soil, sand, stone)
        public List<int> Grid { get; set; }  // RLE compressed grid for terrain
        public List<string> FloorPalette { get; set; }  // Constructed floor types (wood, concrete, etc.)
        public List<int> FloorGrid { get; set; }  // RLE compressed grid for floors
    }
}
