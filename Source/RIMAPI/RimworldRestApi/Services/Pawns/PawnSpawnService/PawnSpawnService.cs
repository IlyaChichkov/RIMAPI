using System;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Services
{
    public class PawnSpawnService : IPawnSpawnService
    {
        public PawnSpawnService() { }

        public ApiResult<PawnSpawnDto> SpawnPawn(PawnSpawnRequestDto request)
        {
            try
            {
                LogApi.Info($"[SpawnPawn] Starting generation for Kind: {request.PawnKind}");

                // 1. Resolve Map
                Map map = null;
                if (!string.IsNullOrEmpty(request.MapId) && int.TryParse(request.MapId, out int mapId))
                {
                    map = MapHelper.GetMapByID(mapId);
                }
                if (map == null) map = Find.CurrentMap;
                if (map == null) return ApiResult<PawnSpawnDto>.Fail("No valid map found to spawn pawn.");

                // 2. Resolve PawnKindDef
                PawnKindDef kindDef = PawnKindDefOf.Colonist;
                if (!string.IsNullOrEmpty(request.PawnKind))
                {
                    kindDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(request.PawnKind);
                    if (kindDef == null)
                        return ApiResult<PawnSpawnDto>.Fail($"Invalid PawnKind: {request.PawnKind}");
                }

                // 3. Resolve Faction
                Faction faction = Faction.OfPlayer;
                PawnGenerationContext context = PawnGenerationContext.PlayerStarter;

                if (!string.IsNullOrEmpty(request.Faction))
                {
                    if (request.Faction.Equals("Player", StringComparison.OrdinalIgnoreCase) ||
                        request.Faction.Equals("PlayerColony", StringComparison.OrdinalIgnoreCase))
                    {
                        faction = Faction.OfPlayer;
                        context = PawnGenerationContext.PlayerStarter;
                    }
                    else
                    {
                        faction = Find.FactionManager.AllFactionsListForReading
                            .FirstOrDefault(f => f.def.defName == request.Faction || f.Name == request.Faction);

                        context = PawnGenerationContext.NonPlayer;
                    }
                }

                // 4. Resolve Xenotype
                XenotypeDef xenotype = null;
                if (ModsConfig.BiotechActive && !string.IsNullOrEmpty(request.Xenotype))
                {
                    xenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail(request.Xenotype);
                }

                // 5. Build Generation Request
                PawnGenerationRequest genRequest = new PawnGenerationRequest(
                    kind: kindDef,
                    faction: faction,
                    context: context,
                    tile: -1,
                    forceGenerateNewPawn: true,
                    allowDead: request.AllowDead,
                    allowDowned: request.AllowDowned,
                    canGeneratePawnRelations: request.CanGeneratePawnRelations,
                    mustBeCapableOfViolence: request.MustBeCapableOfViolence,
                    colonistRelationChanceFactor: 1f,
                    forceAddFreeWarmLayerIfNeeded: false,
                    allowGay: request.AllowGay,
                    allowPregnant: request.AllowPregnant,
                    allowFood: request.AllowFood,
                    allowAddictions: request.AllowAddictions,
                    inhabitant: request.Inhabitant,
                    certainlyBeenInCryptosleep: false,
                    forceRedressWorldPawnIfFormerColonist: false,
                    worldPawnFactionDoesntMatter: false,
                    biocodeWeaponChance: 0f,
                    biocodeApparelChance: 0f,
                    extraPawnForExtraRelationChance: null,
                    relationWithExtraPawnChanceFactor: 1f,
                    validatorPreGear: null,
                    validatorPostGear: null,
                    forcedTraits: null,
                    prohibitedTraits: null,
                    minChanceToRedressWorldPawn: null,
                    fixedBiologicalAge: request.BiologicalAge > 0 ? request.BiologicalAge : (float?)null,
                    fixedChronologicalAge: request.ChronologicalAge > 0 ? request.ChronologicalAge : (float?)null,
                    fixedGender: ParseGender(request.Gender),
                    fixedLastName: null,
                    fixedBirthName: null,
                    fixedTitle: null,
                    forcedXenotype: xenotype
                );

                // 6. Generate
                Pawn pawn = PawnGenerator.GeneratePawn(genRequest);

                // 7. Apply Name Overrides (SAFE VERSION)
                if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.NickName) || !string.IsNullOrEmpty(request.LastName))
                {
                    // FIX: Handle case where pawn.Name is null (Mechanoids) to avoid crash
                    string currentName = pawn.Name?.ToStringShort ?? pawn.Label;

                    string first = !string.IsNullOrEmpty(request.FirstName) ? request.FirstName : currentName;
                    string last = !string.IsNullOrEmpty(request.LastName) ? request.LastName : "";
                    string nick = !string.IsNullOrEmpty(request.NickName) ? request.NickName : first;

                    pawn.Name = new NameTriple(first, nick, last);
                }

                // 8. Determine Spawn Position
                IntVec3 spawnPos;
                if (request.Position != null)
                {
                    spawnPos = new IntVec3(request.Position.X, request.Position.Y, request.Position.Z);
                    spawnPos = spawnPos.ClampInsideMap(map);
                }
                else
                {
                    RCellFinder.TryFindRandomPawnEntryCell(out spawnPos, map, 0.5f);
                }

                if (!spawnPos.Standable(map))
                {
                    CellFinder.TryFindRandomCellNear(spawnPos, map, 5, (IntVec3 c) => c.Standable(map), out spawnPos);
                }

                // 9. Spawn
                GenSpawn.Spawn(pawn, spawnPos, map, WipeMode.Vanish);

                // FIX: Log using Label, not Name (Name can be null)
                LogApi.Info($"[SpawnPawn] Successfully spawned {pawn.Label} (ID: {pawn.thingIDNumber}) at {spawnPos}");

                var result = new PawnSpawnDto
                {
                    PawnId = pawn.thingIDNumber,
                    Name = pawn.Name != null ? pawn.Name.ToStringShort : pawn.Label,
                };

                return ApiResult<PawnSpawnDto>.Ok(result);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error spawning pawn: {ex}");
                return ApiResult<PawnSpawnDto>.Fail($"Failed to spawn pawn: {ex.Message}");
            }
        }

        private Gender? ParseGender(string genderStr)
        {
            if (string.IsNullOrEmpty(genderStr)) return null;
            if (Enum.TryParse<Gender>(genderStr, true, out Gender result))
            {
                return result;
            }
            return null;
        }
    }
}