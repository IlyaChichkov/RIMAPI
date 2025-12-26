using System.Collections.Generic;
using RIMAPI.Models;

namespace RIMAPI.Models
{
    // --- Request Objects ---
    public class CopyAreaRequestDto
    {
        public int MapId { get; set; }
        public PositionDto PointA { get; set; }
        public PositionDto PointB { get; set; }
    }

    public class PasteAreaRequestDto
    {
        public int MapId { get; set; }
        public PositionDto Position { get; set; } // This will be the bottom-left corner
        public BlueprintDto Blueprint { get; set; }
        public bool ClearObstacles { get; set; } = true; // Destroy existing things before pasting
    }

    // --- The Blueprint Data Structure ---
    public class BlueprintDto
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<SavedTerrainDto> Floors { get; set; } = new List<SavedTerrainDto>();
        public List<SavedBuildingDto> Buildings { get; set; } = new List<SavedBuildingDto>();
    }

    public class SavedTerrainDto
    {
        public string DefName { get; set; }
        public int RelX { get; set; }
        public int RelZ { get; set; }
    }

    public class SavedBuildingDto
    {
        public string DefName { get; set; }
        public string StuffDefName { get; set; } // Material (Wood, Steel, etc)
        public int RelX { get; set; }
        public int RelZ { get; set; }
        public int Rotation { get; set; } // 0=North, 1=East, 2=South, 3=West
    }
}