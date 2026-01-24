using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Helpers
{
    public static class PawnHelper
    {
        public static Pawn FindPawnById(string id)
        {
            int.TryParse(id, out int pawnId);
            return FindPawnById(pawnId);
        }

        public static Pawn FindPawnById(int id)
        {
            // Search all maps
            foreach (var map in Find.Maps)
            {
                var pawn = map.mapPawns.AllPawns
                        .FirstOrDefault(p => p.ThingID.Equals(id.ToString()) || p.thingIDNumber == id);
                if (pawn != null) return pawn;
            }

            // Search world pawns (caravans, traveling, etc)
            return Find.WorldPawns.AllPawnsAliveOrDead
                    .FirstOrDefault(p => p.ThingID.Equals(id.ToString()) || p.thingIDNumber == id);
        }

        public static PawnDto PawnToDto(Pawn pawn)
        {
            return new PawnDto
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

        public static PawnInventoryDto GetPawnInventory(Pawn pawn)
        {
            if (pawn == null) return null;

            var inventory = new PawnInventoryDto
            {
                Items = new List<ThingDto>(),
                Apparels = new List<ThingDto>(),
                Equipment = new List<ThingDto>()
            };

            // Inventory (Backpack)
            if (pawn.inventory != null && pawn.inventory.innerContainer != null)
            {
                foreach (var item in pawn.inventory.innerContainer)
                {
                    inventory.Items.Add(ResourcesHelper.ThingToDto(item));
                }
            }

            // Apparel (Clothes)
            if (pawn.apparel != null && pawn.apparel.WornApparel != null)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    inventory.Apparels.Add(ResourcesHelper.ThingToDto(apparel));
                }
            }

            // Equipment (Weapons)
            if (pawn.equipment != null && pawn.equipment.AllEquipmentListForReading != null)
            {
                foreach (var equip in pawn.equipment.AllEquipmentListForReading)
                {
                    inventory.Equipment.Add(ResourcesHelper.ThingToDto(equip));
                }
            }

            return inventory;
        }

        public static PawnDetailedDto PawnToDetailedDto(Pawn pawn)
        {
            try
            {
                return new PawnDetailedDto
                {
                    Sleep = pawn.needs.rest?.CurLevel ?? 0,
                    Comfort = pawn.needs.comfort?.CurLevel ?? 0,
                    Beauty = pawn.needs.beauty?.CurLevel ?? 0,
                    Joy = pawn.needs.joy?.CurLevel ?? 0,
                    Energy = pawn.needs.energy?.CurLevel ?? 0,
                    DrugsDesire = pawn.needs.drugsDesire?.CurLevel ?? 0,
                    SurroundingBeauty = pawn.needs.beauty?.CurLevel ?? 0,
                    FreshAir = pawn.needs.outdoors?.CurLevel ?? 0,
                    WorkInfo = new WorkInfoDto
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
                    PoliciesInfo = new PoliciesInfoDto
                    {
                        FoodPolicyId = pawn.foodRestriction?.CurrentFoodPolicy?.id ?? 0,
                        HostilityResponse = (int)pawn.playerSettings.hostilityResponse,
                    },
                    MedicalInfo = new MedicalInfoDto
                    {
                        Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
                        Hediffs = GetHediffs(pawn),
                        MedicalPolicyId = (int)(
                            pawn.playerSettings?.medCare ?? MedicalCareCategory.NoCare
                        ),
                        IsSelfTendAllowed = pawn.playerSettings?.selfTend ?? false,
                    },
                    SocialInfo = CreatePawnSocialInfoDto(pawn),
                };
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error converting pawn to DTO - {ex.Message}");
                return null;
            }
        }

        public static SocialInfoDto CreatePawnSocialInfoDto(Pawn pawn)
        {
            var dto = new SocialInfoDto
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
    }
}
