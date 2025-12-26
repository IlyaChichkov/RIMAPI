using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Services
{
    public class PawnInfoService : IPawnInfoService
    {
        // --- Helper: Find Any Pawn (Not just colonists) ---
        private Pawn GetPawnById(int id)
        {
            // Search all maps
            foreach (var map in Find.Maps)
            {
                var pawn = map.mapPawns.AllPawns.FirstOrDefault(p => p.ThingID.Equals(id.ToString()) || p.thingIDNumber == id);
                if (pawn != null) return pawn;
            }

            // Search world pawns (caravans, traveling, etc)
            return Find.WorldPawns.AllPawnsAliveOrDead.FirstOrDefault(p => p.ThingID.Equals(id.ToString()) || p.thingIDNumber == id);
        }

        // --- 1. Get List of Pawns on Map ---
        public ApiResult<List<ColonistDto>> GetPawnsOnMap(int mapId)
        {
            try
            {
                var map = MapHelper.GetMapByID(mapId);
                if (map == null) return ApiResult<List<ColonistDto>>.Fail($"Map {mapId} not found.");

                var result = new List<ColonistDto>();

                // Get ALL pawns (colonists, prisoners, enemies, animals, mechs)
                foreach (var pawn in map.mapPawns.AllPawns)
                {
                    result.Add(MapPawnToSummary(pawn));
                }

                return ApiResult<List<ColonistDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error getting pawns on map: {ex}");
                return ApiResult<List<ColonistDto>>.Fail(ex.Message);
            }
        }

        // --- 2. Get Detailed Pawn Info ---
        public ApiResult<ColonistDetailedDto> GetPawnDetails(int pawnId)
        {
            try
            {
                var pawn = GetPawnById(pawnId);
                if (pawn == null) return ApiResult<ColonistDetailedDto>.Fail($"Pawn {pawnId} not found.");

                var details = new ColonistDetailedDto
                {
                    Colonist = MapPawnToSummary(pawn),

                    // Needs (Check for nulls as animals/mechs might not have them)
                    Sleep = pawn.needs?.rest?.CurLevelPercentage ?? 0f,
                    Comfort = pawn.needs?.comfort?.CurLevelPercentage ?? 0f,
                    SurroundingBeauty = pawn.needs?.beauty?.CurLevelPercentage ?? 0f, // Mapped to SurroundingBeauty in DTO?
                    FreshAir = pawn.needs?.outdoors?.CurLevelPercentage ?? 0f,

                    // Sub-components
                    ColonistWorkInfo = GetWorkInfoInternal(pawn),
                    ColonistMedicalInfo = GetMedicalInfoInternal(pawn),
                    ColonistSocialInfo = GetSocialInfoInternal(pawn),
                    ColonistPoliciesInfo = GetPoliciesInfoInternal(pawn)
                };

                return ApiResult<ColonistDetailedDto>.Ok(details);
            }
            catch (Exception ex)
            {
                return ApiResult<ColonistDetailedDto>.Fail(ex.Message);
            }
        }

        public ApiResult<ColonistInventoryDto> GetPawnInventory(int pawnId)
        {
            try
            {
                var pawn = GetPawnById(pawnId);
                if (pawn == null) return ApiResult<ColonistInventoryDto>.Fail($"Pawn {pawnId} not found.");

                var inventory = new ColonistInventoryDto
                {
                    Items = new List<ThingDto>(),
                    Apparels = new List<ThingDto>(),
                    Equipment = new List<ThingDto>()
                };

                // Inventory (Backpack)
                if (pawn.inventory != null && pawn.inventory.innerContainer != null)
                {
                    foreach (var item in pawn.inventory.innerContainer)
                        inventory.Items.Add(ResourcesHelper.ThingToDto(item));
                }

                // Apparel (Clothes)
                if (pawn.apparel != null && pawn.apparel.WornApparel != null)
                {
                    foreach (var app in pawn.apparel.WornApparel)
                        inventory.Apparels.Add(ResourcesHelper.ThingToDto(app));
                }

                // Equipment (Weapons)
                if (pawn.equipment != null && pawn.equipment.AllEquipmentListForReading != null)
                {
                    foreach (var equip in pawn.equipment.AllEquipmentListForReading)
                        inventory.Equipment.Add(ResourcesHelper.ThingToDto(equip));
                }

                return ApiResult<ColonistInventoryDto>.Ok(inventory);
            }
            catch (Exception ex)
            {
                return ApiResult<ColonistInventoryDto>.Fail(ex.Message);
            }
        }

        // --- Internal Mappers ---

        private ColonistDto MapPawnToSummary(Pawn pawn)
        {
            return new ColonistDto
            {
                Id = pawn.thingIDNumber,
                Name = pawn.Name?.ToStringShort ?? pawn.Label,
                Gender = pawn.gender.ToString(),
                Age = pawn.ageTracker.AgeBiologicalYears,
                Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 0f,
                Mood = pawn.needs?.mood?.CurLevelPercentage ?? 0f, // Null for animals/mechs
                Hunger = pawn.needs?.food?.CurLevelPercentage ?? 0f,
                Position = new PositionDto { X = pawn.Position.x, Y = pawn.Position.y, Z = pawn.Position.z }
            };
        }

        private ColonistWorkInfoDto GetWorkInfoInternal(Pawn pawn)
        {
            var info = new ColonistWorkInfoDto
            {
                CurrentJob = pawn.CurJob?.def?.reportString ?? "Idle",
                Skills = new List<SkillDto>(),
                Traits = new List<TraitDto>(),
                WorkPriorities = new List<WorkPriorityDto>()
            };

            // Skills (Only humans/mechs with skill trackers)
            if (pawn.skills != null)
            {
                foreach (var skill in pawn.skills.skills)
                {
                    info.Skills.Add(new SkillDto
                    {
                        Name = skill.def.defName,
                        Level = skill.Level,
                        XpProgressPercent = skill.XpProgressPercent,
                        Passion = (int)skill.passion,
                        Description = skill.def.description
                    });
                }
            }

            // Traits (Only humans)
            if (pawn.story != null && pawn.story.traits != null)
            {
                foreach (var trait in pawn.story.traits.allTraits)
                {
                    info.Traits.Add(new TraitDto
                    {
                        Name = trait.def.defName,
                        Label = trait.Label,
                        Description = trait.TipString(pawn)
                    });
                }
            }

            return info;
        }

        private ColonistMedicalInfoDto GetMedicalInfoInternal(Pawn pawn)
        {
            var info = new ColonistMedicalInfoDto
            {
                Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 0f,

                // --- NEW LOGIC ---
                IsDead = pawn.Dead,
                IsDowned = pawn.Downed, // Checks pain shock, consciousness, legs, etc.

                // Safe access to capacities (Animals/Mechs might differ, but usually valid)
                Consciousness = pawn.health?.capacities?.GetLevel(PawnCapacityDefOf.Consciousness) ?? 0f,
                Moving = pawn.health?.capacities?.GetLevel(PawnCapacityDefOf.Moving) ?? 0f,
                // -----------------

                Hediffs = new List<HediffDto>(),
                IsSelfTendAllowed = pawn.playerSettings?.selfTend ?? false
            };

            if (pawn.health != null && pawn.health.hediffSet != null)
            {
                foreach (var h in pawn.health.hediffSet.hediffs)
                {
                    info.Hediffs.Add(new HediffDto
                    {
                        DefName = h.def.defName,
                        Label = h.Label,
                        Severity = h.Severity,
                        PartLabel = h.Part?.Label,
                        Bleeding = h.Bleeding,
                        IsPermanent = h.IsPermanent()
                    });
                }
            }
            return info;
        }

        private ColonistSocialInfoDto GetSocialInfoInternal(Pawn pawn)
        {
            var info = new ColonistSocialInfoDto
            {
                Id = pawn.ThingID,
                Name = pawn.Name?.ToStringShort,
                DirectRelations = new List<RelationDto>()
            };

            if (pawn.relations != null)
            {
                foreach (var rel in pawn.relations.DirectRelations)
                {
                    info.DirectRelations.Add(new RelationDto
                    {
                        relationDefName = rel.def.defName,
                        otherPawnId = rel.otherPawn.ThingID,
                        otherPawnName = rel.otherPawn.Name?.ToStringShort
                    });
                }
            }
            return info;
        }

        private ColonistPoliciesInfoDto GetPoliciesInfoInternal(Pawn pawn)
        {
            // Policies are specific to Colonists (Pawn.foodRestriction, etc.)
            // We return safe defaults for non-colonists
            return new ColonistPoliciesInfoDto
            {
                FoodPolicyId = pawn.foodRestriction?.CurrentFoodPolicy?.id ?? -1,
                HostilityResponse = (int)(pawn.playerSettings?.hostilityResponse ?? HostilityResponseMode.Flee)
            };
        }
    }
}