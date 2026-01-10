using System.Collections.Generic;

namespace RIMAPI.Models.Map
{
    public class OreDataDto
    {
        public int MapWidth { get; set; }

        // Key = DefName (e.g., "MineableSteel", "Granite")
        public Dictionary<string, OreGroupDto> Ores { get; set; }
    }

    public class OreGroupDto
    {
        public int MaxHp { get; set; } // Send MaxHP once per type

        // Flattened Map Indices: (z * MapWidth) + x
        public List<int> Cells { get; set; }

        // HP values matching the order of Cells
        public List<int> Hp { get; set; }
    }
}
