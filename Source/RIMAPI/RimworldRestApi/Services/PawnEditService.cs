using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RIMAPI.Services
{
    public class PawnEditService : IPawnEditService
    {
        public PawnEditService() { }
        public ApiResult EditPawn(PawnEditRequestDto request)
        {
            try
            {
                LogApi.Info("[EditPawn] GetPawnById");
                Pawn pawn = ColonistsHelper.FindPawnById(request.PawnId.ToString());
                if (pawn == null)
                {
                    return ApiResult.Fail($"Pawn with ID {request.PawnId} not found.");
                }

                LogApi.Info("[EditPawn] Name");
                // --- Basic properties ---
                if (!string.IsNullOrEmpty(request.Name))
                {
                    // Ensure we handle different name types correctly
                    if (pawn.Name is NameTriple triple)
                        pawn.Name = new NameTriple(triple.First, request.Name, triple.Last);
                    else
                        pawn.Name = new NameSingle(request.Name);
                }

                LogApi.Info("[EditPawn] Gender");
                if (!string.IsNullOrEmpty(request.Gender))
                {
                    if (Enum.TryParse<Gender>(request.Gender, true, out Gender newGender))
                    {
                        pawn.gender = newGender;
                    }
                    else
                    {
                        return ApiResult.Fail($"Invalid gender: {request.Gender}");
                    }
                }

                LogApi.Info("[EditPawn] ageTracker");
                if (pawn.ageTracker != null)
                {
                    if (request.BiologicalAge.HasValue)
                        pawn.ageTracker.AgeBiologicalTicks = (long)request.BiologicalAge.Value * 3600000L;

                    if (request.ChronologicalAge.HasValue)
                        pawn.ageTracker.AgeChronologicalTicks = (long)request.ChronologicalAge.Value * 3600000L;
                }


                LogApi.Info("[EditPawn] Health");
                // --- Health ---
                if (request.HealAllInjuries.HasValue && request.HealAllInjuries.Value)
                {
                    HealthUtility.HealNonPermanentInjuriesAndRestoreLegs(pawn);
                }

                if (request.RemoveAllDiseases.HasValue && request.RemoveAllDiseases.Value && pawn.health?.hediffSet != null)
                {
                    foreach (var hediff in pawn.health.hediffSet.hediffs.ToList())
                    {
                        if (hediff.def != null && hediff.def.HasComp(typeof(HediffComp_Immunizable)))
                        {
                            pawn.health.RemoveHediff(hediff);
                        }
                    }
                }


                LogApi.Info("[EditPawn] Needs");
                // --- Needs (Check if needs tracker exists) ---
                if (pawn.needs != null)
                {
                    if (request.Hunger.HasValue)
                    {
                        var food = pawn.needs.food;
                        if (food != null) food.CurLevelPercentage = Mathf.Clamp01(request.Hunger.Value);
                    }

                    if (request.Rest.HasValue)
                    {
                        var rest = pawn.needs.rest;
                        if (rest != null) rest.CurLevelPercentage = Mathf.Clamp01(request.Rest.Value);
                    }

                    if (request.Mood.HasValue)
                    {
                        // Try setting mood directly if possible, otherwise fall back to Joy
                        var mood = pawn.needs.mood;
                        if (mood != null)
                        {
                            mood.CurLevelPercentage = Mathf.Clamp01(request.Mood.Value);
                        }

                        var joy = pawn.needs.joy;
                        if (joy != null) joy.CurLevelPercentage = Mathf.Clamp01(request.Mood.Value);
                    }
                }

                LogApi.Info("[EditPawn] Skills");
                // --- Skills (Check if skills tracker exists - animals/mechs don't have this) ---
                if (request.Skills != null && pawn.skills != null)
                {
                    foreach (var skillEntry in request.Skills)
                    {
                        var skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skillEntry.Key);
                        if (skillDef != null && skillEntry.Value.HasValue)
                        {
                            SkillRecord skill = pawn.skills.GetSkill(skillDef);
                            if (skill != null)
                            {
                                skill.Level = skillEntry.Value.Value;
                                skill.xpSinceLastLevel = skill.XpRequiredForLevelUp / 2f;
                            }
                        }
                    }
                }

                LogApi.Info("[EditPawn] Traits");
                // --- Traits (Check if story tracker exists - animals/mechs don't have this) ---
                if (pawn.story != null && pawn.story.traits != null)
                {
                    if (request.RemoveTraits != null)
                    {
                        foreach (var traitName in request.RemoveTraits)
                        {
                            var traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(traitName);
                            if (traitDef != null)
                            {
                                Trait traitToRemove = pawn.story.traits.GetTrait(traitDef);
                                if (traitToRemove != null)
                                {
                                    pawn.story.traits.RemoveTrait(traitToRemove);
                                }
                            }
                        }
                    }

                    if (request.AddTraits != null)
                    {
                        foreach (var traitName in request.AddTraits)
                        {
                            var traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(traitName);
                            if (traitDef != null)
                            {
                                if (!pawn.story.traits.HasTrait(traitDef))
                                {
                                    Trait traitToAdd = new Trait(traitDef);
                                    pawn.story.traits.GainTrait(traitToAdd);
                                }
                            }
                        }
                    }
                }

                LogApi.Info("[EditPawn] Equipment");
                // --- Equipment & Inventory ---
                if (request.DropAllEquipment.HasValue && request.DropAllEquipment.Value && pawn.equipment != null)
                {
                    pawn.equipment.DropAllEquipment(pawn.Position);
                }

                if (request.DropAllInventory.HasValue && request.DropAllInventory.Value && pawn.inventory != null)
                {
                    pawn.inventory.DropAllNearPawn(pawn.Position);
                }

                LogApi.Info("[EditPawn] Status");
                // --- Status (Drafting) ---
                if (pawn.drafter != null)
                {
                    if (request.Draft.HasValue)
                    {
                        pawn.drafter.Drafted = request.Draft.Value;
                    }
                    if (request.Undraft.HasValue)
                    {
                        pawn.drafter.Drafted = !request.Undraft.Value;
                    }
                }

                LogApi.Info("[EditPawn] Kill");
                if (request.Kill.HasValue && request.Kill.Value && !pawn.Dead)
                {
                    pawn.Kill(null);
                }

                if (request.Resurrect.HasValue && request.Resurrect.Value && pawn.Dead)
                {
                    ResurrectionUtility.TryResurrect(pawn);
                }

                LogApi.Info("[EditPawn] Position");
                // --- Position ---
                if (request.ChangePosition && request.Position != null)
                {
                    Map targetMap = pawn.Map; // Default to current map
                    LogApi.Info("[EditPawn] newPos");
                    IntVec3 newPos = new IntVec3(request.Position.X, request.Position.Y, request.Position.Z);

                    if (!string.IsNullOrEmpty(request.MapId))
                    {
                        LogApi.Info("[EditPawn] MapId");
                        if (int.TryParse(request.MapId, out int mapId))
                        {
                            var foundMap = MapHelper.GetMapByID(mapId);
                            if (foundMap != null) targetMap = foundMap;
                        }
                    }

                    LogApi.Info("[EditPawn] targetMap");
                    // Only teleport if we have a valid map
                    if (targetMap != null)
                    {
                        TeleportPawn(pawn, newPos, targetMap);
                    }
                }

                LogApi.Info("[EditPawn] Faction");
                // --- Faction & Relations ---
                if (!string.IsNullOrEmpty(request.Faction))
                {
                    Faction newFaction = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(f => f.def.defName == request.Faction || f.Name == request.Faction);
                    if (newFaction != null)
                    {
                        pawn.SetFaction(newFaction);
                    }
                    // If faction not found, we log but continue
                }

                LogApi.Info("[EditPawn] Faction");
                // Handle Guest/Prisoner status (Check guest tracker)
                if (pawn.guest != null)
                {
                    if (request.MakeColonist.HasValue && request.MakeColonist.Value)
                    {
                        if (pawn.Faction != Faction.OfPlayer)
                        {
                            pawn.SetFaction(Faction.OfPlayer);
                        }
                        RecruitUtility.Recruit(pawn, Faction.OfPlayer);
                    }

                    if (request.MakePrisoner.HasValue && request.MakePrisoner.Value)
                    {
                        pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
                    }

                    if (request.ReleasePrisoner.HasValue && request.ReleasePrisoner.Value && pawn.IsPrisoner)
                    {
                        pawn.guest.SetGuestStatus(null, GuestStatus.Guest);
                        // Usually requires additional logic to actually "release" them to walk off map, 
                        // but this resets their status.
                    }
                }

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error editing pawn: {ex}"); // Log full stack trace
                return ApiResult.Fail($"Failed to edit pawn: {ex.Message}");
            }
        }

        public static bool TeleportPawn(Pawn pawn, IntVec3 newPosition, Map map = null)
        {
            if (map == null)
                map = pawn.Map;

            // Ensure position is valid  
            newPosition = newPosition.ClampInsideMap(map);

            if (!newPosition.Standable(map))
            {
                // Find nearest standable cell if target is blocked  
                CellFinder.TryFindRandomCellNear(newPosition, map, 5,
                    (IntVec3 c) => c.Standable(map), out newPosition);
            }

            pawn.Position = newPosition;
            pawn.Notify_Teleported(endCurrentJob: true, resetTweenedPos: false);

            return true;
        }
    }
}