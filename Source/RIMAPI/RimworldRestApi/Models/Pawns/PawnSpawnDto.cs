namespace RIMAPI.Models
{
    public class PawnSpawnRequestDto
    {
        // Definitions
        public string PawnKind { get; set; } = "Colonist"; // Default to Colonist
        public string Faction { get; set; } = "PlayerColony"; // Default to Player
        public string Xenotype { get; set; } // Biotech DLC

        // Biological details
        public string Gender { get; set; } // Male, Female, None
        public float? BiologicalAge { get; set; }
        public float? ChronologicalAge { get; set; }

        // Other details
        public bool AllowDead { get; set; } = false;
        public bool AllowDowned { get; set; } = false;
        public bool CanGeneratePawnRelations { get; set; } = true;
        public bool MustBeCapableOfViolence { get; set; } = false;
        public bool AllowGay { get; set; } = false;
        public bool AllowPregnant { get; set; } = false;
        public bool AllowFood { get; set; } = false;
        public bool AllowAddictions { get; set; } = false;
        public bool Inhabitant { get; set; } = false;

        // Optional override name
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }

        // Location
        public string MapId { get; set; }
        public PositionDto Position { get; set; } // Helper class for X,Y,Z
    }

    public class PawnSpawnDto
    {
        public int PawnId { get; set; }
        public string Name { get; set; }
    }
}

