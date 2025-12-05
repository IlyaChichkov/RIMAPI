using System.Collections.Generic;
using System.Linq;
using Mono.Posix;
using Verse.AI.Group;

namespace RIMAPI.Models
{
    public class QuestsDto
    {
        public List<ActiveQuestDto> ActiveQuests { get; set; } = new List<ActiveQuestDto>();
        public List<ActiveQuestDto> HistoricalQuests { get; set; } = new List<ActiveQuestDto>();
    }

    public class IncidentsDto
    {
        public List<IncidentDto> Incidents { get; set; } = new List<IncidentDto>();
    }

    public class DefsDto
    {
        public List<IncidentDefDto> IncidentsDefs { get; set; } = new List<IncidentDefDto>();
        public List<GameConditionDefDto> ConditionsDefs { get; set; } =
            new List<GameConditionDefDto>();
    }

    public class IncidentDto
    {
        public string IncidentDef { get; set; }
        public string Label { get; set; }
        public string Category { get; set; }
        public float IncidentHour { get; set; }
        public float DaysSinceOccurred { get; set; }
    }

    public class IncidentDefDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float BaseChance { get; set; }
        public float BaseChanceWithRoyalty { get; set; }
        public string LetterDefName { get; set; }
        public string PopulationEffect { get; set; }
        public string LetterText { get; set; }
        public int MinPopulation { get; set; }
        public float MinThreatPoints { get; set; }
        public float MaxThreatPoints { get; set; }
        public string Category { get; set; }
        public bool ShouldIgnoreRecentWeighting { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> DisallowedBiomes { get; set; } = new List<string>();
        public List<string> AllowedBiomes { get; set; } = new List<string>();
        public bool RequireColonistsPresent { get; set; }
    }

    public class GameConditionDefDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string LetterText { get; set; }
    }

    public class ActiveQuestDto
    {
        public string QuestDef { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public float ExpiryHours { get; set; }
        public List<string> Reward { get; set; }
    }

    public class TriggerIncidentRequestDto
    {
        public string Name { get; set; }
        public string MapId { get; set; }
        public IncidentParmsDto IncidentParms { get; set; }
    }

    public class LordDto
    {
        public int LoadId { get; set; }
        public string FactionName { get; set; }
        public string FactionDefName { get; set; }
        public string LordJobType { get; set; }
        public string CurrentToilName { get; set; }
        public int TicksInToil { get; set; }
        public int NumPawnsLostViolently { get; set; }
        public int NumPawnsEverGained { get; set; }
        public List<string> OwnedPawnIds { get; set; }
        public List<string> OwnedBuildingIds { get; set; }
        public List<string> QuestTags { get; set; }
        public string InSignalLeave { get; set; }
        public bool AnyActivePawn { get; set; }

        public static LordDto ToDto(Lord lord)
        {
            return new LordDto
            {
                LoadId = lord.loadID,
                FactionName = lord.faction?.Name,
                FactionDefName = lord.faction?.def?.defName,
                LordJobType = lord.LordJob?.GetType().Name,
                CurrentToilName = lord.CurLordToil?.GetType().Name,
                TicksInToil = lord.ticksInToil,
                NumPawnsLostViolently = lord.numPawnsLostViolently,
                NumPawnsEverGained = lord.numPawnsEverGained,
                OwnedPawnIds = lord.ownedPawns.Select(p => p.ThingID).ToList(),
                OwnedBuildingIds = lord.ownedBuildings.Select(b => b.ThingID).ToList(),
                QuestTags = lord.questTags?.ToList(),
                InSignalLeave = lord.inSignalLeave,
                AnyActivePawn = lord.AnyActivePawn,
            };
        }
    }

    public class IncidentParmsDto
    {
        // Basic incident parameters
        public string Target { get; set; }
        public float Points { get; set; } = -1f;
        public string Faction { get; set; }
        public bool Forced { get; set; }

        // Letter parameters
        public string CustomLetterLabel { get; set; }
        public string CustomLetterText { get; set; }
        public string CustomLetterDef { get; set; }
        public bool SendLetter { get; set; } = true;
        public List<string> LetterHyperlinkThingDefs { get; set; }
        public List<string> LetterHyperlinkHediffDefs { get; set; }

        // Signal and spawn parameters
        public string InSignalEnd { get; set; }
        public bool Silent { get; set; }
        public string SpawnCenter { get; set; }
        public string SpawnRotation { get; set; }

        // Raid and combat parameters
        public bool GenerateFightersOnly { get; set; }
        public bool DontUseSingleUseRocketLaunchers { get; set; }
        public string RaidStrategy { get; set; }
        public string RaidArrivalMode { get; set; }
        public bool RaidForceOneDowned { get; set; }
        public bool RaidNeverFleeIndividual { get; set; }
        public bool RaidArrivalModeForQuickMilitaryAid { get; set; }
        public string RaidAgeRestriction { get; set; }

        // Biocode parameters
        public float BiocodeWeaponsChance { get; set; }
        public float BiocodeApparelChance { get; set; }

        // Pawn group parameters
        public Dictionary<string, int> PawnGroups { get; set; }
        public int? PawnGroupMakerSeed { get; set; }
        public string PawnIdeo { get; set; }
        public string Lord { get; set; }
        public string PawnKind { get; set; }
        public int PawnCount { get; set; }
        public string PawnGroupKind { get; set; }
        public string TraderKind { get; set; }

        // Quest parameters
        public string Quest { get; set; }
        public string QuestScriptDef { get; set; }
        public string QuestTag { get; set; }

        // Mech cluster parameters
        public string MechClusterSketch { get; set; }

        // Behavior parameters
        public bool CanTimeoutOrFlee { get; set; } = true;
        public bool CanSteal { get; set; } = true;
        public bool CanKidnap { get; set; } = true;

        // Controller and location parameters
        public string ControllerPawn { get; set; }
        public string InfestationLocOverride { get; set; }

        // Target and gift parameters
        public List<string> AttackTargets { get; set; }
        public List<string> Gifts { get; set; }

        // Body size parameters
        public float TotalBodySize { get; set; }

        // Psychic ritual parameters
        public string PsychicRitualDef { get; set; }

        // Multiplier parameters
        public float PointMultiplier { get; set; } = 1f;
        public bool BypassStorytellerSettings { get; set; }

        // Generated pawns
        public List<string> StoreGeneratedNeutralPawns { get; set; }

        // Computed property
        public int PawnGroupCount { get; set; }

        // Pod opening delay
        public int PodOpenDelay { get; set; } = 140;
    }
}
