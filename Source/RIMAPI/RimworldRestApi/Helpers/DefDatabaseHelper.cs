using System.Linq;
using RIMAPI.Core;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Helpers
{
    public static class DefDatabaseHelper
    {
        public static IncidentParms IncidentParmsFromDto(
            IncidentParmsDto dto,
            IIncidentTarget target = null
        )
        {
            if (dto == null)
                return null;

            // Create base parms using DefaultParmsNow if we have a category and target
            IncidentParms parms;

            if (target != null)
            {
                // If you have incident category available, use DefaultParmsNow
                // parms = StorytellerUtility.DefaultParmsNow(incidentCategory, target);
                // For now, create basic parms
                parms = new IncidentParms();
                parms.target = target;
            }
            else
            {
                parms = new IncidentParms();
            }

            // Basic parameters
            parms.points = dto.Points;
            parms.forced = dto.Forced;

            // Letter parameters
            parms.customLetterLabel = dto.CustomLetterLabel;
            parms.customLetterText = dto.CustomLetterText;
            parms.sendLetter = dto.SendLetter;
            parms.inSignalEnd = dto.InSignalEnd;
            parms.silent = dto.Silent;

            // // Spawn parameters
            // if (!string.IsNullOrEmpty(dto.SpawnCenter) && IntVec3.TryParse(dto.SpawnCenter, out IntVec3 spawnCenter))
            // {
            //     parms.spawnCenter = spawnCenter;
            // }

            // if (!string.IsNullOrEmpty(dto.SpawnRotation) && Rot4.TryParse(dto.SpawnRotation, out Rot4 spawnRotation))
            // {
            //     parms.spawnRotation = spawnRotation;
            // }

            // Raid and combat parameters
            parms.generateFightersOnly = dto.GenerateFightersOnly;
            parms.dontUseSingleUseRocketLaunchers = dto.DontUseSingleUseRocketLaunchers;
            parms.raidForceOneDowned = dto.RaidForceOneDowned;
            parms.raidNeverFleeIndividual = dto.RaidNeverFleeIndividual;
            parms.raidArrivalModeForQuickMilitaryAid = dto.RaidArrivalModeForQuickMilitaryAid;

            // Biocode parameters
            parms.biocodeWeaponsChance = dto.BiocodeWeaponsChance;
            parms.biocodeApparelChance = dto.BiocodeApparelChance;

            // Pawn parameters
            parms.pawnGroupMakerSeed = dto.PawnGroupMakerSeed;
            parms.pawnCount = dto.PawnCount;
            parms.podOpenDelay = dto.PodOpenDelay;

            // Quest parameters
            parms.questTag = dto.QuestTag;

            // Behavior parameters
            parms.canTimeoutOrFlee = dto.CanTimeoutOrFlee;
            parms.canSteal = dto.CanSteal;
            parms.canKidnap = dto.CanKidnap;

            // Body size and multiplier parameters
            parms.totalBodySize = dto.TotalBodySize;
            parms.pointMultiplier = dto.PointMultiplier;
            parms.bypassStorytellerSettings = dto.BypassStorytellerSettings;

            // Debug faction lookup
            if (!string.IsNullOrEmpty(dto.Faction))
            {
                LogApi.Message($"Looking up faction: {dto.Faction}");

                parms.faction = Find.FactionManager.AllFactions.FirstOrDefault(f =>
                    f.def.defName.ToLower() == dto.Faction.ToLower()
                );
                LogApi.Message($"Case-insensitive result: {parms.faction?.def?.defName}");
            }

            if (!string.IsNullOrEmpty(dto.CustomLetterDef))
            {
                parms.customLetterDef = DefDatabase<LetterDef>.GetNamedSilentFail(
                    dto.CustomLetterDef
                );
            }

            if (!string.IsNullOrEmpty(dto.RaidStrategy))
            {
                parms.raidStrategy = DefDatabase<RaidStrategyDef>.GetNamedSilentFail(
                    dto.RaidStrategy
                );
            }

            if (!string.IsNullOrEmpty(dto.RaidArrivalMode))
            {
                parms.raidArrivalMode = DefDatabase<PawnsArrivalModeDef>.GetNamedSilentFail(
                    dto.RaidArrivalMode
                );
            }

            if (!string.IsNullOrEmpty(dto.RaidAgeRestriction))
            {
                parms.raidAgeRestriction = DefDatabase<RaidAgeRestrictionDef>.GetNamedSilentFail(
                    dto.RaidAgeRestriction
                );
            }

            if (!string.IsNullOrEmpty(dto.PawnKind))
            {
                parms.pawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(dto.PawnKind);
            }

            if (!string.IsNullOrEmpty(dto.PawnGroupKind))
            {
                parms.pawnGroupKind = DefDatabase<PawnGroupKindDef>.GetNamedSilentFail(
                    dto.PawnGroupKind
                );
            }

            if (!string.IsNullOrEmpty(dto.TraderKind))
            {
                parms.traderKind = DefDatabase<TraderKindDef>.GetNamed(dto.TraderKind, false);
            }

            if (!string.IsNullOrEmpty(dto.QuestScriptDef))
            {
                parms.questScriptDef = DefDatabase<QuestScriptDef>.GetNamedSilentFail(
                    dto.QuestScriptDef
                );
            }

            if (!string.IsNullOrEmpty(dto.PsychicRitualDef))
            {
                parms.psychicRitualDef = DefDatabase<PsychicRitualDef>.GetNamedSilentFail(
                    dto.PsychicRitualDef
                );
            }

            // Look up collections of defs
            if (dto.LetterHyperlinkThingDefs != null)
            {
                parms.letterHyperlinkThingDefs = dto
                    .LetterHyperlinkThingDefs.Select(defName =>
                        DefDatabase<ThingDef>.GetNamedSilentFail(defName)
                    )
                    .Where(def => def != null)
                    .ToList();
            }

            if (dto.LetterHyperlinkHediffDefs != null)
            {
                parms.letterHyperlinkHediffDefs = dto
                    .LetterHyperlinkHediffDefs.Select(defName =>
                        DefDatabase<HediffDef>.GetNamedSilentFail(defName)
                    )
                    .Where(def => def != null)
                    .ToList();
            }

            // Look up pawns by ID (this is more complex and may require searching)
            if (dto.ControllerPawn != null)
            {
                parms.controllerPawn = FindPawnById(dto.ControllerPawn);
            }

            // Look up infestation location
            // if (!string.IsNullOrEmpty(dto.InfestationLocOverride) &&
            //     IntVec3.TryParse(dto.InfestationLocOverride, out IntVec3 infestationLoc))
            // {
            //     parms.infestationLocOverride = infestationLoc;
            // }

            return parms;
        }

        // Helper method to find pawn by ID
        private static Pawn FindPawnById(string pawnId)
        {
            if (string.IsNullOrEmpty(pawnId))
                return null;

            // Search in maps
            foreach (var map in Find.Maps)
            {
                var pawn = map.mapPawns.AllPawns.FirstOrDefault(p => p.GetUniqueLoadID() == pawnId);
                if (pawn != null)
                    return pawn;
            }

            // Search in world pawns
            return Find.World.worldPawns.AllPawnsAlive.FirstOrDefault(p =>
                p.GetUniqueLoadID() == pawnId
            );
        }
    }
}
