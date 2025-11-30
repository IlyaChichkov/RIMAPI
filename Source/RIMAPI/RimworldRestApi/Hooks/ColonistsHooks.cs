using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimworldRestApi.Hooks
{
    /// <summary>
    /// Patch Thing.Ingested to notify when a colonist eats something.
    /// </summary>
    [HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
    public static class IngestedPatch
    {
        static void Postfix(Thing __instance, Pawn ingester, float nutritionWanted, float __result)
        {
            try
            {
                if (ingester == null || !ingester.IsColonist || __instance == null)
                    return;

                float hungerBefore = 0f;
                float hungerAfter = 0f;
                if (ingester.needs?.food != null)
                {
                    hungerBefore = ingester.needs.food.CurLevelPercentage;
                    hungerAfter = Mathf.Clamp01(hungerBefore + __result);
                }

                string foodType = "Unknown";
                var ingestibleProps = __instance.def.ingestible;
                if (ingestibleProps != null)
                {
                    foodType = ingestibleProps.foodType.ToString();
                }

                float nutrition = 0f;
                try
                {
                    nutrition = __instance.GetStatValue(StatDefOf.Nutrition);
                }
                catch
                { /* Ignore */
                }

                RIMAPI.Core.LogApi.Info("Send colonist_ate EVENT");
                EventPublisherAccess.Publish(
                    "colonist_ate",
                    new
                    {
                        colonist = new
                        {
                            name = ingester.Name?.ToStringShort ?? "Unknown",
                            hungerBefore = hungerBefore,
                            hungerAfter = hungerAfter,
                        },
                        food = new
                        {
                            defName = __instance.def.defName ?? "Unknown",
                            label = __instance.Label ?? "Unknown",
                            nutrition = nutrition,
                            foodType = foodType,
                        },
                        ticks = Find.TickManager?.TicksGame ?? 0,
                    }
                );
            }
            catch (Exception ex)
            {
                RIMAPI.Core.LogApi.Error($"[RimworldRestApi] Error in IngestedPatch.Postfix: {ex}");
            }
        }
    }

    /// <summary>
    /// Patch GenRecipe.MakeRecipeProducts to notify when a recipe produces items.
    /// </summary>
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class MakeRecipeProductsPatch
    {
        static void Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker)
        {
            try
            {
                if (worker == null || recipeDef == null || __result == null)
                    return;

                EventPublisherAccess.Publish(
                    "make_recipe_product",
                    new
                    {
                        worker = new
                        {
                            id = worker.thingIDNumber,
                            name = worker.Name?.ToStringShort ?? "Unknown",
                        },
                        result = __result
                            .Where(t => t != null)
                            .Select(t => new
                            {
                                thing_id = t.thingIDNumber,
                                def_name = t.def?.defName ?? "Unknown",
                                label = t.Label,
                                nutrition = t.GetStatValue(StatDefOf.Nutrition, true),
                            }),
                        recipeDef = new { def_name = recipeDef.defName },
                        ticks = Find.TickManager.TicksGame,
                    }
                );
            }
            catch (Exception ex)
            {
                RIMAPI.Core.LogApi.Error(
                    $"[RimworldRestApi] Error in MakeRecipeProductsPatch.Postfix: {ex}"
                );
            }
        }
    }

    /// <summary>
    /// Patch UnfinishedThing.Destroy to notify when an unfinished item is destroyed.
    /// </summary>
    [HarmonyPatch(typeof(UnfinishedThing), "Destroy")]
    public static class UnfinishedDestroyPatch
    {
        static void Postfix(UnfinishedThing __instance, DestroyMode mode)
        {
            try
            {
                if (__instance == null)
                    return;

                var bill = __instance.BoundBill;
                Thing billGiverThing = bill?.billStack?.billGiver as Thing;

                EventPublisherAccess.Publish(
                    "unfinished_destroyed",
                    new
                    {
                        unfinished = new
                        {
                            id = __instance.thingIDNumber,
                            def_name = __instance.def?.defName ?? "Unknown",
                            label = __instance.LabelNoCount,
                            work_left = __instance.workLeft,
                            stuff = __instance.Stuff?.defName,
                        },
                        bill = bill != null
                            ? new
                            {
                                recipe_def = bill.recipe?.defName,
                                repeat_mode = bill.repeatMode.ToString(),
                                suspended = bill.suspended,
                            }
                            : null,
                        billGiver = billGiverThing != null
                            ? new
                            {
                                id = billGiverThing.thingIDNumber,
                                def_name = billGiverThing.def?.defName,
                                label = billGiverThing.LabelNoCount,
                            }
                            : null,
                        destroy_mode = mode.ToString(),
                        map_id = __instance.Map?.uniqueID,
                        ticks = Find.TickManager.TicksGame,
                    }
                );
            }
            catch (Exception ex)
            {
                RIMAPI.Core.LogApi.Error(
                    $"[RimworldRestApi] Error in UnfinishedDestroyPatch.Postfix: {ex}"
                );
            }
        }
    }

    /// <summary>
    /// Sends a "date_changed" event only when in-game day, season or year changes
    /// </summary>
    [HarmonyPatch(typeof(DateNotifier), nameof(DateNotifier.DateNotifierTick))]
    public static class DateChangePatch
    {
        private static int _lastDayOfYear = -1;
        private static Season _lastSeason = Season.Undefined;
        private static int _lastYear = -1;

        static void Postfix(DateNotifier __instance)
        {
            try
            {
                Map map = Find.CurrentMap;
                if (map == null)
                    return;

                Vector2 longLat = Find.WorldGrid.LongLatOf(map.Tile);
                long absTicks = Find.TickManager.TicksAbs;

                int dayOfYear = GenDate.DayOfYear(absTicks, longLat.x);
                int year = GenDate.Year(absTicks, longLat.x);
                Season season = GenDate.Season(absTicks, longLat);

                bool dayChanged = dayOfYear != _lastDayOfYear;
                bool seasonChanged = season != _lastSeason;
                bool yearChanged = year != _lastYear;

                if (!dayChanged && !seasonChanged && !yearChanged)
                    return;

                _lastDayOfYear = dayOfYear;
                _lastSeason = season;
                _lastYear = year;

                string fullDateWithHour = GenDate.DateReadoutStringAt(absTicks, longLat);

                EventPublisherAccess.Publish(
                    "date_changed",
                    new
                    {
                        date = new
                        {
                            full = fullDateWithHour,
                            dayOfYear = dayOfYear,
                            year = year,
                            season = season.ToString(),
                        },
                        map = new
                        {
                            id = map.uniqueID,
                            tile = map.Tile,
                            longitude = longLat.x,
                            latitude = longLat.y,
                        },
                        ticksAbs = absTicks,
                        ticks = Find.TickManager.TicksGame,
                    }
                );
            }
            catch (Exception ex)
            {
                RIMAPI.Core.LogApi.Error(
                    $"[RimworldRestApi] Error in DateChangePatch.Postfix: {ex}"
                );
            }
        }
    }

    // You can add more patches here - they all use the same simple pattern
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class PawnKilledPatch
    {
        static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            try
            {
                if (__instance == null)
                    return;

                EventPublisherAccess.Publish(
                    "pawn_killed",
                    new
                    {
                        pawn = new
                        {
                            id = __instance.thingIDNumber,
                            name = __instance.Name?.ToStringShort ?? "Unknown",
                            isColonist = __instance.IsColonist,
                            faction = __instance.Faction?.Name ?? "Unknown",
                        },
                        cause = dinfo?.Def?.defName ?? "Unknown",
                        ticks = Find.TickManager.TicksGame,
                    }
                );
            }
            catch (Exception ex)
            {
                RIMAPI.Core.LogApi.Error(
                    $"[RimworldRestApi] Error in PawnKilledPatch.Postfix: {ex}"
                );
            }
        }
    }
}
