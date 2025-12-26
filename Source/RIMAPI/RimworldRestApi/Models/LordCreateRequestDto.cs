using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class LordCreateRequestDto
    {
        // Who owns this squad?
        public string Faction { get; set; }
        public string MapId { get; set; }

        // Who joins this squad?
        public List<int> PawnIds { get; set; }

        // What should they do?
        // Options: "AssaultColony", "AssaultThings", "DefendPoint", "Wander"
        public string JobType { get; set; } = "AssaultColony";

        // For "DefendPoint"
        public PositionDto Position { get; set; }

        // For "AssaultThings" (List of Enemy Pawn IDs to attack)
        public List<int> TargetIds { get; set; }
    }

    public class LordCreateDto
    {
        public int LordId { get; set; }
        public int MemberCount { get; set; }
    }
}