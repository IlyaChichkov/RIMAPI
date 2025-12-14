using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Helpers
{
    public static class ColonistsHelper
    {
        public static Pawn GetPawnById(int id)
        {
            Pawn pawn = Find
                .CurrentMap.mapPawns.AllPawns.Where(p => p.thingIDNumber == id)
                .FirstOrDefault();
            if (pawn == null)
            {
                throw new Exception($"Failed to find pawn with id: {id}");
            }
            return pawn;
        }

        public static List<ColonistDto> GetColonists()
        {
            var colonists = new List<ColonistDto>();

            try
            {
                var map = Find.CurrentMap;
                if (map == null)
                    return colonists;

                var freeColonists = map.mapPawns?.FreeColonists;
                if (freeColonists == null)
                    return colonists;

                foreach (var pawn in freeColonists)
                {
                    if (pawn == null)
                        continue;

                    colonists.Add(CreateColonistDto(pawn));
                }
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error getting colonists - {ex.Message}");
            }

            return colonists;
        }

        public static List<PawnPositionDto> GetColonistPositions()
        {
            var positions = new List<PawnPositionDto>();

            try
            {
                var map = Find.CurrentMap;
                if (map == null)
                    return positions;

                var freeColonists = map.mapPawns?.FreeColonists;
                if (freeColonists == null)
                    return positions;

                foreach (var pawn in freeColonists)
                {
                    if (pawn == null || !pawn.Spawned)
                        continue;

                    positions.Add(new PawnPositionDto
                    {
                        Id = pawn.thingIDNumber,
                        MapId = map.uniqueID,
                        X = pawn.Position.x,
                        Z = pawn.Position.z
                    });
                }
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error getting colonist positions - {ex.Message}");
            }

            return positions;
        }

        public static ColonistDto CreateColonistDto(Pawn pawn)
        {
            return new ColonistDto
            {
                Id = pawn.thingIDNumber,
                Name = pawn.Name?.ToStringShort ?? "Unknown",
                Gender = pawn.gender.ToString(),
                Age = pawn.ageTracker.AgeBiologicalYears,
                Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
                Mood = pawn.needs?.mood?.CurLevelPercentage ?? 0.5f,
                Hunger = pawn.needs.food?.CurLevel ?? 0,
                Position = new PositionDto
                {
                    X = pawn.Position.x,
                    Y = pawn.Position.y,
                    Z = pawn.Position.z,
                },
            };
        }

        public static List<ColonistDetailedDto> GetColonistsDetailed()
        {
            var colonists = new List<ColonistDetailedDto>();

            try
            {
                var map = Find.CurrentMap;
                if (map == null)
                    return colonists;

                var freeColonists = map.mapPawns?.FreeColonists;
                if (freeColonists == null)
                    return colonists;

                foreach (var pawn in freeColonists)
                {
                    if (pawn == null)
                        continue;

                    colonists.Add(PawnToDetailedDto(pawn));
                }
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error getting detailed colonists - {ex.Message}");
            }

            return colonists;
        }

        public static ColonistDetailedDto PawnToDetailedDto(Pawn pawn)
        {
            try
            {
                return new ColonistDetailedDto
                {
                    Sleep = pawn.needs.rest?.CurLevel ?? 0,
                    Comfort = pawn.needs.comfort?.CurLevel ?? 0,
                    SurroundingBeauty = pawn.needs.beauty?.CurLevel ?? 0,
                    FreshAir = pawn.needs.outdoors?.CurLevel ?? 0,
                    Colonist = CreateColonistDto(pawn),
                    ColonistWorkInfo = new ColonistWorkInfoDto
                    {
                        Skills =
                            pawn.skills.skills?.Where(skill => skill != null && skill.def != null)
                                .Select(skill => new SkillDto
                                {
                                    Name = skill.def.defName,
                                    Level = skill.Level,
                                    Description = skill.def.description,
                                    MinLevel = SkillRecord.MinLevel,
                                    MaxLevel = SkillRecord.MaxLevel,
                                    LevelDescriptor = skill.LevelDescriptor,
                                    PermanentlyDisabled = skill.PermanentlyDisabled,
                                    TotallyDisabled = skill.TotallyDisabled,
                                    XpTotalEarned = skill.XpTotalEarned,
                                    XpProgressPercent = skill.XpProgressPercent,
                                    XpRequiredForLevelUp = skill.XpRequiredForLevelUp,
                                    XpSinceLastLevel = skill.xpSinceLastLevel,
                                    Aptitude = skill.Aptitude,
                                    Passion = (int)skill.passion,
                                    DisabledWorkTags = (int)skill.def.disablingWorkTags,
                                })
                                .ToList() ?? new List<SkillDto>(),
                        CurrentJob = pawn.CurJob?.def?.defName ?? "",
                        Traits = GetTraits(pawn),
                        WorkPriorities = GetWorkPriorities(pawn),
                    },
                    ColonistPoliciesInfo = new ColonistPoliciesInfoDto
                    {
                        FoodPolicyId = pawn.foodRestriction?.CurrentFoodPolicy?.id ?? 0,
                        HostilityResponse = (int)pawn.playerSettings.hostilityResponse,
                    },
                    ColonistMedicalInfo = new ColonistMedicalInfoDto
                    {
                        Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
                        Hediffs = GetHediffs(pawn),
                        MedicalPolicyId = (int)(
                            pawn.playerSettings?.medCare ?? MedicalCareCategory.NoCare
                        ),
                        IsSelfTendAllowed = pawn.playerSettings?.selfTend ?? false,
                    },
                    ColonistSocialInfo = CreatePawnSocialInfoDto(pawn),
                };
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error converting pawn to DTO - {ex.Message}");
                return new ColonistDetailedDto
                {
                    Colonist = new ColonistDto { Id = pawn.thingIDNumber, Name = "Error" },
                };
            }
        }

        public static ColonistSocialInfoDto CreatePawnSocialInfoDto(Pawn pawn)
        {
            var dto = new ColonistSocialInfoDto
            {
                Id = pawn.ThingID,
                Name = pawn.Name?.ToString(),
                DirectRelations = new List<RelationDto>(),
                ChildrenCount = pawn.relations.ChildrenCount,
            };

            foreach (var relation in pawn.relations.DirectRelations)
            {
                dto.DirectRelations.Add(
                    new RelationDto
                    {
                        relationDefName = relation.def.defName,
                        otherPawnId = relation.otherPawn.ThingID,
                        otherPawnName = relation.otherPawn.Name?.ToString(),
                    }
                );
            }
            return dto;
        }

        public static List<HediffDto> GetHediffs(Pawn pawn)
        {
            try
            {
                return pawn.health?.hediffSet?.hediffs?.Where(h => h != null)
                        .Select(h => HediffToDto(h))
                        .ToList() ?? new List<HediffDto>();
            }
            catch
            {
                return new List<HediffDto>();
            }
        }

        public static HediffDto HediffToDto(Hediff hediff)
        {
            if (hediff == null)
                return null;

            var dto = new HediffDto
            {
                LoadId = hediff.loadID,
                DefName = hediff.def?.defName,
                Label = hediff.Label,
                LabelCap = hediff.LabelCap,
                LabelInBrackets = hediff.LabelInBrackets,

                Severity = hediff.Severity,
                SeverityLabel = hediff.SeverityLabel,
                CurStageIndex = hediff.CurStageIndex,
                CurStageLabel = hediff.CurStage?.label,

                PartLabel = hediff.Part?.Label,
                PartDefName = hediff.Part?.def?.defName,

                AgeTicks = hediff.ageTicks,
                AgeString = hediff.ageTicks.ToStringTicksToPeriod(),

                Visible = hediff.Visible,
                IsPermanent = hediff.IsPermanent(),
                IsTended = hediff.IsTended(),
                TendableNow = hediff.TendableNow(),
                Bleeding = hediff.Bleeding,
                BleedRate = hediff.BleedRate,
                IsLethal = hediff.IsLethal,
                IsCurrentlyLifeThreatening = hediff.IsCurrentlyLifeThreatening,
                CanEverKill = hediff.CanEverKill(),

                SourceDefName = hediff.sourceDef?.defName,
                SourceLabel = hediff.sourceDef?.label,
                SourceBodyPartGroupDefName = hediff.sourceBodyPartGroup?.defName,
                SourceHediffDefName = hediff.sourceHediffDef?.defName,

                CombatLogText = hediff.combatLogText,
                TipStringExtra = hediff.TipStringExtra,
                PainFactor = hediff.PainFactor,
                PainOffset = hediff.PainOffset,
            };

            return dto;
        }

        public static List<TraitDto> GetTraits(Pawn pawn)
        {
            try
            {
                return pawn.story?.traits?.allTraits?.Where(t => t != null)
                        .Select(t => new TraitDto
                        {
                            Name = t.def.defName,
                            Label = t.Label,
                            Description = t.def.description,
                            DisabledWorkTags = (int)t.def.disabledWorkTags,
                            Suppressed = t.Suppressed,
                        })
                        .ToList() ?? new List<TraitDto>();
            }
            catch
            {
                return new List<TraitDto>();
            }
        }

        public static List<WorkPriorityDto> GetWorkPriorities(Pawn pawn)
        {
            var priorities = new List<WorkPriorityDto>();

            try
            {
                if (pawn.workSettings == null)
                    return priorities;

                foreach (WorkTypeDef workType in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    if (workType == null)
                        continue;

                    var priority = pawn.workSettings.GetPriority(workType);
                    if (priority > 0)
                    {
                        priorities.Add(
                            new WorkPriorityDto
                            {
                                WorkType = workType.defName,
                                Priority = priority,
                                IsTotallyDisabled = pawn.WorkTypeIsDisabled(workType),
                            }
                        );
                    }
                }

                return priorities.OrderBy(p => p.Priority).ToList();
            }
            catch (Exception ex)
            {
                Core.LogApi.Error(
                    $"Error getting work priorities for pawn {pawn.thingIDNumber} - {ex.Message}"
                );
                return priorities;
            }
        }

        public static ThingFilterDto GetThingFilterDto(ThingFilter filter)
        {
            var disallowedFilters = DefDatabase<SpecialThingFilterDef>
                .AllDefs.Where(sf => !filter.Allows(sf))
                .Select(sf => sf.defName)
                .ToList();

            return new ThingFilterDto
            {
                AllowedThingDefNames = filter.AllowedThingDefs.Select(d => d.defName).ToList(),
                DisallowedSpecialFilterDefNames = disallowedFilters,
                AllowedHitPointsMin = filter.AllowedHitPointsPercents.min,
                AllowedHitPointsMax = filter.AllowedHitPointsPercents.max,
                AllowedQualityMin = filter.AllowedQualityLevels.min.ToString(),
                AllowedQualityMax = filter.AllowedQualityLevels.max.ToString(),
                AllowedHitPointsConfigurable = filter.allowedHitPointsConfigurable,
                AllowedQualitiesConfigurable = filter.allowedQualitiesConfigurable,
            };
        }

        public static List<OutfitDto> GetOutfits()
        {
            List<OutfitDto> outfits = new List<OutfitDto>();

            foreach (var policy in Current.Game.outfitDatabase.AllOutfits)
            {
                outfits.Add(
                    new OutfitDto
                    {
                        Id = policy.id,
                        Label = policy.label,
                        Filter = GetThingFilterDto(policy.filter),
                    }
                );
            }

            return outfits;
        }

        public static void EditPawn(PawnEditRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    Core.LogApi.Error("EditPawn: Request is null");
                    return;
                }

                Core.LogApi.Message($"EditPawn: Looking for pawn with ID: {request.PawnId}");

                var pawn = FindPawnById(request.PawnId.ToString());

                if (pawn == null)
                {
                    Core.LogApi.Error($"EditPawn: Could not find pawn with ID: {request.PawnId}");

                    // Debug: List available pawns
                    DebugAvailablePawns();
                    return;
                }

                Core.LogApi.Message(
                    $"EditPawn: Found pawn: {pawn.Label} (ID: {pawn.GetUniqueLoadID()}, ThingID: {pawn.thingIDNumber})"
                );

                ApplyPawnEdits(pawn, request);
                Core.LogApi.Message($"EditPawn: Successfully edited pawn: {pawn.Label}");
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Failed to edit pawn: {ex.Message}");
                Core.LogApi.Error($"Stack trace: {ex.StackTrace}");
            }
        }

        public static void DebugAvailablePawns()
        {
            try
            {
                Core.LogApi.Message("=== AVAILABLE PAWNS ===");

                // Check maps
                foreach (var map in Find.Maps)
                {
                    Core.LogApi.Message($"Map: {map.uniqueID}");

                    foreach (var pawn in map.mapPawns.AllPawns) // Limit to first 5 to avoid spam
                    {
                        Core.LogApi.Message(
                            $"  - {pawn.Label} (ID: {pawn.GetUniqueLoadID()}, ThingID: {pawn.thingIDNumber})"
                        );
                    }

                    if (map.mapPawns.AllPawns.Count() > 5)
                    {
                        Core.LogApi.Message($"  ... and {map.mapPawns.AllPawns.Count() - 5} more");
                    }
                }

                // Check world pawns
                var worldPawns = Find.WorldPawns.AllPawnsAlive.ToList(); // Limit to first 3
                Core.LogApi.Message($"World pawns: {worldPawns.Count} (showing first 3)");
                foreach (var pawn in worldPawns)
                {
                    Core.LogApi.Message(
                        $"  - {pawn.Label} (ID: {pawn.GetUniqueLoadID()}, ThingID: {pawn.thingIDNumber})"
                    );
                }

                Core.LogApi.Message("=== END AVAILABLE PAWNS ===");
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error debugging available pawns: {ex.Message}");
            }
        }

        public static Pawn FindPawnById(string pawnId)
        {
            if (string.IsNullOrEmpty(pawnId))
                return null;

            try
            {
                // Search in all maps
                foreach (var map in Find.Maps)
                {
                    var pawn = map.mapPawns.AllPawns.FirstOrDefault(p =>
                        (p.GetUniqueLoadID() == pawnId)
                        || (p.ThingID == pawnId)
                        || (p.thingIDNumber.ToString() == pawnId)
                        || (p.Label?.ToLower().Contains(pawnId.ToLower()) == true)
                    ); // Also try matching by name

                    if (pawn != null)
                    {
                        Core.LogApi.Message($"Found pawn in map: {pawn.Label}");
                        return pawn;
                    }
                }

                // Search in world pawns
                var worldPawn = Find.WorldPawns.AllPawnsAlive.FirstOrDefault(p =>
                    (p.GetUniqueLoadID() == pawnId)
                    || (p.thingIDNumber.ToString() == pawnId)
                    || (p.Label?.ToLower().Contains(pawnId.ToLower()) == true)
                );

                if (worldPawn != null)
                {
                    Core.LogApi.Message($"Found pawn in world pawns: {worldPawn.Label}");
                    return worldPawn;
                }

                return null;
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error in FindPawnById: {ex.Message}");
                return null;
            }
        }

        public static void ApplyPawnEdits(Pawn pawn, PawnEditRequestDto request)
        {
            if (pawn == null)
            {
                Core.LogApi.Error("ApplyPawnEdits: Pawn is null");
                return;
            }

            try
            {
                Core.LogApi.Message($"Starting to apply edits to pawn: {pawn.Label}");

                // Name
                if (!string.IsNullOrEmpty(request.Name))
                {
                    pawn.Name = new NameSingle(request.Name);
                    Core.LogApi.Message($"Set name to: {request.Name}");
                }

                // Gender
                if (!string.IsNullOrEmpty(request.Gender))
                {
                    if (Enum.TryParse<Gender>(request.Gender, true, out var gender))
                    {
                        pawn.gender = gender;
                        Core.LogApi.Message($"Set gender to: {gender}");
                    }
                    else
                    {
                        Core.LogApi.Error($"Invalid gender value: {request.Gender}");
                    }
                }

                // Age - Add safety checks
                if (request.BiologicalAge.HasValue)
                {
                    if (pawn.ageTracker != null)
                    {
                        pawn.ageTracker.AgeBiologicalTicks = (long)(
                            request.BiologicalAge.Value * 3600000f
                        );
                        Core.LogApi.Message($"Set biological age to: {request.BiologicalAge}");
                    }
                    else
                    {
                        Core.LogApi.Error("Pawn ageTracker is null");
                    }
                }

                if (request.ChronologicalAge.HasValue)
                {
                    if (pawn.ageTracker != null)
                    {
                        pawn.ageTracker.AgeChronologicalTicks = (long)(
                            request.ChronologicalAge.Value * 3600000f
                        );
                        Core.LogApi.Message(
                            $"Set chronological age to: {request.ChronologicalAge}"
                        );
                    }
                }

                // Health operations
                if (request.HealAllInjuries == true)
                {
                    if (pawn.health != null)
                    {
                        var injuries = pawn
                            .health.hediffSet.hediffs.Where(h => h.def.isBad && h.Visible)
                            .ToList();

                        foreach (var injury in injuries)
                        {
                            pawn.health.RemoveHediff(injury);
                        }
                        Core.LogApi.Message($"Healed {injuries.Count} injuries");
                    }
                    else
                    {
                        Core.LogApi.Error("Pawn health is null");
                    }
                }

                if (request.RemoveAllDiseases == true)
                {
                    if (pawn.health != null)
                    {
                        var diseases = pawn
                            .health.hediffSet.hediffs.Where(h => h.def.makesSickThought)
                            .ToList();

                        foreach (var disease in diseases)
                        {
                            pawn.health.RemoveHediff(disease);
                        }
                        Core.LogApi.Message($"Removed {diseases.Count} diseases");
                    }
                }

                // Needs with safety checks
                if (request.Hunger.HasValue)
                {
                    if (pawn.needs?.food != null)
                    {
                        pawn.needs.food.CurLevel = request.Hunger.Value;
                        Core.LogApi.Message($"Set hunger to: {request.Hunger.Value:P0}");
                    }
                }

                if (request.Rest.HasValue)
                {
                    if (pawn.needs?.rest != null)
                    {
                        pawn.needs.rest.CurLevel = request.Rest.Value;
                        Core.LogApi.Message($"Set rest to: {request.Rest.Value:P0}");
                    }
                }

                if (request.Mood.HasValue)
                {
                    if (pawn.needs?.mood != null)
                    {
                        pawn.needs.mood.CurLevel = request.Mood.Value;
                        Core.LogApi.Message($"Set mood to: {request.Mood.Value:P0}");
                    }
                }

                // Skills with safety checks
                if (request.Skills != null)
                {
                    if (pawn.skills != null)
                    {
                        foreach (var skillPair in request.Skills)
                        {
                            var skillDef = DefDatabase<SkillDef>.AllDefs.FirstOrDefault(s =>
                                s.defName.Equals(skillPair.Key, StringComparison.OrdinalIgnoreCase)
                            );

                            if (skillDef != null && skillPair.Value.HasValue)
                            {
                                var skill = pawn.skills.GetSkill(skillDef);
                                if (skill != null)
                                {
                                    skill.Level = Mathf.Clamp(skillPair.Value.Value, 0, 20);
                                    Core.LogApi.Message(
                                        $"Set {skillDef.LabelCap} to level {skillPair.Value}"
                                    );
                                }
                            }
                        }
                    }
                }

                // Draft/Undraft
                if (request.Draft == true)
                {
                    if (pawn.drafter != null && !pawn.Drafted)
                    {
                        pawn.drafter.Drafted = true;
                        Core.LogApi.Message("Drafted pawn");
                    }
                }

                if (request.Undraft == true)
                {
                    if (pawn.drafter != null && pawn.Drafted)
                    {
                        pawn.drafter.Drafted = false;
                        Core.LogApi.Message("Undrafted pawn");
                    }
                }

                // Kill
                if (request.Kill == true && !pawn.Dead)
                {
                    pawn.Kill(null);
                    Core.LogApi.Message("Killed pawn");
                }

                // Faction changes
                if (!string.IsNullOrEmpty(request.Faction))
                {
                    var faction = Find.FactionManager.AllFactions.FirstOrDefault(f =>
                        f.def.defName.Equals(request.Faction, StringComparison.OrdinalIgnoreCase)
                        || f.Name.Equals(request.Faction, StringComparison.OrdinalIgnoreCase)
                    );

                    if (faction != null)
                    {
                        pawn.SetFaction(faction);
                        Core.LogApi.Message($"Set faction to: {faction.Name}");
                    }
                }

                if (request.MakeColonist == true)
                {
                    pawn.SetFaction(Faction.OfPlayer);
                    Core.LogApi.Message("Made colonist");
                }

                // Prisoner operations
                if (request.MakePrisoner == true)
                {
                    if (pawn.guest != null)
                    {
                        pawn.guest.SetGuestStatus(Faction.OfPlayer, RimWorld.GuestStatus.Prisoner);
                        Core.LogApi.Message("Made prisoner");
                    }
                }

                if (request.ReleasePrisoner == true)
                {
                    if (pawn.guest != null && pawn.IsPrisonerOfColony)
                    {
                        pawn.guest.SetGuestStatus(null);
                        Core.LogApi.Message("Released prisoner");
                    }
                }

                // Equipment & Inventory
                if (request.DropAllEquipment == true)
                {
                    if (pawn.equipment != null)
                    {
                        pawn.equipment.DropAllEquipment(pawn.Position);
                        Core.LogApi.Message("Dropped all equipment");
                    }
                }

                if (request.DropAllInventory == true)
                {
                    if (pawn.inventory != null)
                    {
                        pawn.inventory.DropAllNearPawn(pawn.Position);
                        Core.LogApi.Message("Dropped all inventory");
                    }
                }

                Core.LogApi.Message($"Finished applying edits to pawn: {pawn.Label}");
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error in ApplyPawnEdits: {ex.Message}");
                Core.LogApi.Error($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
