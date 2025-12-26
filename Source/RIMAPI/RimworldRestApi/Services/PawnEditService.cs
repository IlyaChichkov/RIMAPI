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
        private Pawn GetPawn(int id)
        {
            var pawn = ColonistsHelper.FindPawnById(id.ToString());
            if (pawn == null) throw new ArgumentException($"Pawn with ID {id} not found.");
            return pawn;
        }

        public ApiResult UpdateBasicInfo(PawnBasicRequest request)
        {
            try
            {
                Pawn pawn = GetPawn(request.PawnId);

                if (!string.IsNullOrEmpty(request.Name))
                {
                    if (pawn.Name is NameTriple triple)
                    {
                        string first = !string.IsNullOrEmpty(request.FirstName) ? request.FirstName : triple.First;
                        string last = !string.IsNullOrEmpty(request.LastName) ? request.LastName : triple.Last;
                        string nick = !string.IsNullOrEmpty(request.NickName) ? request.NickName : request.Name;
                        pawn.Name = new NameTriple(first, nick, last);
                    }
                    else
                    {
                        pawn.Name = new NameSingle(request.Name);
                    }
                }

                if (!string.IsNullOrEmpty(request.Gender) && Enum.TryParse(request.Gender, true, out Gender g))
                {
                    pawn.gender = g;
                }

                if (pawn.ageTracker != null)
                {
                    if (request.BiologicalAge.HasValue)
                        pawn.ageTracker.AgeBiologicalTicks = (long)request.BiologicalAge.Value * 3600000L;
                    if (request.ChronologicalAge.HasValue)
                        pawn.ageTracker.AgeChronologicalTicks = (long)request.ChronologicalAge.Value * 3600000L;
                }

                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        public ApiResult UpdateHealth(PawnHealthRequest request)
        {
            try
            {
                Pawn pawn = GetPawn(request.PawnId);

                if (request.HealAllInjuries)
                {
                    HealthUtility.HealNonPermanentInjuriesAndRestoreLegs(pawn);
                }

                if (request.RestoreBodyParts)
                {
                    // Advanced: Restore missing parts
                    pawn.health.RestorePart(null); // Null restores whole body
                }

                if (request.RemoveAllDiseases && pawn.health?.hediffSet != null)
                {
                    foreach (var hediff in pawn.health.hediffSet.hediffs.ToList())
                    {
                        if (hediff.def != null && hediff.def.HasComp(typeof(HediffComp_Immunizable)))
                            pawn.health.RemoveHediff(hediff);
                    }
                }

                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        public ApiResult UpdateNeeds(PawnNeedsRequest request)
        {
            try
            {
                if (request == null) return ApiResult.Fail("Invalid JSON request body.");

                Pawn pawn = GetPawn(request.PawnId);
                if (pawn.needs == null) return ApiResult.Fail("Pawn has no needs tracker.");

                if (request.Food.HasValue && pawn.needs.food != null)
                    pawn.needs.food.CurLevelPercentage = Mathf.Clamp01(request.Food.Value);

                if (request.Rest.HasValue && pawn.needs.rest != null)
                    pawn.needs.rest.CurLevelPercentage = Mathf.Clamp01(request.Rest.Value);

                if (request.Mood.HasValue)
                {
                    if (pawn.needs.mood != null) pawn.needs.mood.CurLevelPercentage = Mathf.Clamp01(request.Mood.Value);
                    // Joy is often linked to mood, but usually handled separately in API. 
                    // Ensure this is intended behavior.
                    if (pawn.needs.joy != null) pawn.needs.joy.CurLevelPercentage = Mathf.Clamp01(request.Mood.Value);
                }

                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        public ApiResult UpdateSkills(PawnSkillsRequest request)
        {
            try
            {
                Pawn pawn = GetPawn(request.PawnId);
                if (pawn.skills == null) return ApiResult.Fail("Pawn has no skills.");

                foreach (var skillDto in request.Skills)
                {
                    var skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skillDto.SkillName);
                    if (skillDef == null) continue;

                    var record = pawn.skills.GetSkill(skillDef);
                    if (record == null) continue;

                    if (skillDto.Level.HasValue)
                    {
                        record.Level = skillDto.Level.Value;
                        record.xpSinceLastLevel = record.XpRequiredForLevelUp / 2f;
                    }

                    if (skillDto.Passion.HasValue)
                    {
                        record.passion = (Passion)skillDto.Passion;
                    }
                }
                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        public ApiResult UpdateTraits(PawnTraitsRequest request)
        {
            try
            {
                Pawn pawn = GetPawn(request.PawnId);
                if (pawn.story?.traits == null) return ApiResult.Fail("Pawn has no traits tracker.");

                // Remove
                if (request.RemoveTraits != null)
                {
                    foreach (var tName in request.RemoveTraits)
                    {
                        var def = DefDatabase<TraitDef>.GetNamedSilentFail(tName);
                        if (def == null) continue;
                        var existing = pawn.story.traits.GetTrait(def);
                        if (existing != null) pawn.story.traits.RemoveTrait(existing);
                    }
                }

                // Add
                if (request.AddTraits != null)
                {
                    foreach (var tDto in request.AddTraits)
                    {
                        var def = DefDatabase<TraitDef>.GetNamedSilentFail(tDto.TraitName);
                        if (def == null) continue;
                        if (!pawn.story.traits.HasTrait(def))
                        {
                            pawn.story.traits.GainTrait(new Trait(def, tDto.Degree ?? 0));
                        }
                    }
                }
                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        public ApiResult UpdateInventory(PawnInventoryRequest request)
        {
            try
            {
                Pawn pawn = GetPawn(request.PawnId);
                if (pawn.inventory == null) return ApiResult.Fail("Pawn has no inventory.");

                if (request.ClearInventory)
                    pawn.inventory.DestroyAll();
                else if (request.DropInventory)
                    pawn.inventory.DropAllNearPawn(pawn.Position);

                if (request.AddItems != null)
                {
                    foreach (var item in request.AddItems)
                    {
                        var def = DefDatabase<ThingDef>.GetNamedSilentFail(item.DefName);
                        if (def != null)
                        {
                            Thing thing = ThingMaker.MakeThing(def);
                            thing.stackCount = item.Count > 0 ? item.Count : 1;
                            pawn.inventory.TryAddItemNotForSale(thing);
                        }
                    }
                }
                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        public ApiResult UpdateApparel(PawnApparelRequest request)
        {
            try
            {
                Pawn pawn = GetPawn(request.PawnId);

                // Equipment (Weapons)
                if (request.DropWeapons && pawn.equipment != null)
                    pawn.equipment.DropAllEquipment(pawn.Position, false);

                // Apparel (Clothes)
                if (request.DropApparel && pawn.apparel != null)
                    pawn.apparel.DropAll(pawn.Position);

                // Note: Adding specific equipped items is complex because of layers/slots, 
                // sticking to Drop/Clear for now unless specific logic requested.

                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        public ApiResult UpdateStatus(PawnStatusRequest request)
        {
            try
            {
                Pawn pawn = GetPawn(request.PawnId);

                if (request.Kill && !pawn.Dead) pawn.Kill(null);
                if (request.Resurrect && pawn.Dead) ResurrectionUtility.TryResurrect(pawn);

                if (pawn.drafter != null && request.IsDrafted.HasValue)
                {
                    pawn.drafter.Drafted = request.IsDrafted.Value;
                }

                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        public ApiResult UpdatePosition(PawnPositionRequest request)
        {
            try
            {
                Pawn pawn = GetPawn(request.PawnId);
                Map map = pawn.Map;

                if (!string.IsNullOrEmpty(request.MapId) && int.TryParse(request.MapId, out int mid))
                {
                    var found = MapHelper.GetMapByID(mid);
                    if (found != null) map = found;
                }

                if (request.Position != null && map != null)
                {
                    IntVec3 pos = new IntVec3(request.Position.X, request.Position.Y, request.Position.Z);
                    TeleportPawn(pawn, pos, map);
                }
                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        public ApiResult UpdateFaction(PawnFactionRequest request)
        {
            try
            {
                Pawn pawn = GetPawn(request.PawnId);

                if (!string.IsNullOrEmpty(request.SetFaction))
                {
                    Faction f = Find.FactionManager.AllFactionsListForReading
                       .FirstOrDefault(x => x.def.defName == request.SetFaction || x.Name == request.SetFaction);
                    if (f != null) pawn.SetFaction(f);
                }

                if (request.MakeColonist && pawn.Faction != Faction.OfPlayer)
                {
                    pawn.SetFaction(Faction.OfPlayer);
                    RecruitUtility.Recruit(pawn, Faction.OfPlayer);
                }

                if (pawn.guest != null)
                {
                    if (request.MakePrisoner) pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
                    if (request.ReleasePrisoner && pawn.IsPrisoner) pawn.guest.SetGuestStatus(null, GuestStatus.Guest);
                }

                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        // Reuse existing teleport logic
        private static void TeleportPawn(Pawn pawn, IntVec3 newPosition, Map map)
        {
            newPosition = newPosition.ClampInsideMap(map);
            if (!newPosition.Standable(map))
                CellFinder.TryFindRandomCellNear(newPosition, map, 5, c => c.Standable(map), out newPosition);

            pawn.Position = newPosition;
            pawn.Notify_Teleported(true, false);
        }
    }
}