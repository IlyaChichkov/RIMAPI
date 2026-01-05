using System.Collections.Generic;

namespace RIMAPI.Models
{
    // --- 1. Basic Info ---
    public class PawnBasicRequest
    {
        public int PawnId { get; set; }
        public string Name { get; set; } // Fallback for simple name
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string Gender { get; set; } // Male, Female, None
        public int? BiologicalAge { get; set; }
        public int? ChronologicalAge { get; set; }
    }

    // --- 2. Health ---
    public class PawnHealthRequest
    {
        public int PawnId { get; set; }
        public bool HealAllInjuries { get; set; }
        public bool RestoreBodyParts { get; set; } // Regrow missing limbs
        public bool RemoveAllDiseases { get; set; }
    }

    // --- 3. Needs ---
    public class PawnNeedsRequest
    {
        public int PawnId { get; set; }
        public float? Food { get; set; } // 0.0 to 1.0
        public float? Rest { get; set; }
        public float? Mood { get; set; }
    }

    // --- 4. Skills ---
    public class PawnSkillsRequest
    {
        public int PawnId { get; set; }
        public List<SkillEntryDto> Skills { get; set; }
    }

    public class SkillEntryDto
    {
        public string SkillName { get; set; } // e.g. "Shooting"
        public int? Level { get; set; } // 0-20
        public int? Passion { get; set; } // 0-2
    }

    // --- 5. Traits ---
    public class PawnTraitsRequest
    {
        public int PawnId { get; set; }
        public List<TraitEntryDto> AddTraits { get; set; }
        public List<string> RemoveTraits { get; set; }
    }

    public class TraitEntryDto
    {
        public string TraitName { get; set; } // e.g. "SpeedOffset"
        public int? Degree { get; set; } // Specific degree (e.g., -1 for Pessimist, 1 for Optimist)
    }

    // --- 6. Inventory ---
    public class PawnInventoryRequest
    {
        public int PawnId { get; set; }
        public bool DropInventory { get; set; }
        public bool ClearInventory { get; set; } // Deletes items entirely
        public List<ItemDto> AddItems { get; set; }
    }

    public class ItemDto
    {
        public string DefName { get; set; } // e.g. "MealSimple"
        public int Count { get; set; }
    }

    // --- 7. Apparel / Equipment ---
    public class PawnApparelRequest
    {
        public int PawnId { get; set; }
        public bool DropApparel { get; set; } // Clothes
        public bool DropWeapons { get; set; } // Equipment
    }

    // --- 8. Status ---
    public class PawnStatusRequest
    {
        public int PawnId { get; set; }
        public bool? IsDrafted { get; set; }
        public bool Kill { get; set; }
        public bool Resurrect { get; set; }
    }

    // --- 9. Position ---
    public class PawnPositionRequest
    {
        public int PawnId { get; set; }
        public string MapId { get; set; } // Optional: Move to another map
        public PositionDto Position { get; set; }
    }

    // --- 10. Faction ---
    public class PawnFactionRequest
    {
        public int PawnId { get; set; }
        public string SetFaction { get; set; } // Faction DefName
        public bool MakeColonist { get; set; } // Auto recruit
        public bool MakePrisoner { get; set; }
        public bool ReleasePrisoner { get; set; }
    }
}