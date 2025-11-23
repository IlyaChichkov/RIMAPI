using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimworldRestApi.Core;
using UnityEngine;
using Verse;
using Verse.AI;

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
                // Comprehensive null checking
                if (ingester == null || !ingester.IsColonist || __instance == null)
                    return;

                if (__instance.def == null)
                    return;

                var sse = SseService.GetService();
                if (sse == null)
                    return;

                // Safe hunger level calculation
                float hungerBefore = 0f;
                float hungerAfter = 0f;
                if (ingester.needs?.food != null)
                {
                    hungerBefore = ingester.needs.food.CurLevelPercentage;
                    hungerAfter = hungerBefore + __result;
                    if (hungerAfter > 1f) hungerAfter = 1f;
                    if (hungerAfter < 0f) hungerAfter = 0f;
                }

                // Safe food type extraction
                string foodType = "Unknown";
                var ingestibleProps = __instance.def.ingestible;
                if (ingestibleProps != null)
                {
                    foodType = ingestibleProps.foodType.ToString();
                }

                // Safe nutrition calculation with error handling
                float nutrition = 0f;
                try
                {
                    nutrition = __instance.GetStatValue(StatDefOf.Nutrition);
                }
                catch (Exception statEx)
                {
                    DebugLogging.Warning($"[RimworldRestApi] Error getting nutrition stat: {statEx.Message}");
                    // Continue with default nutrition value
                }

                var foodEvent = new
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
                        foodType = foodType
                    },
                    ticks = Find.TickManager?.TicksGame ?? 0
                };

                sse.QueueEventBroadcast("colonist_ate", foodEvent);
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[RimworldRestApi] Error in IngestedPatch.Postfix: {ex}");
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
                if (worker == null || recipeDef == null || __result == null) return;

                var sseService = SseService.GetService();
                if (sseService == null) return;

                var payload = new
                {
                    worker = new
                    {
                        id = worker.thingIDNumber,
                        name = worker.Name?.ToStringShort ?? "Unknown",
                    },
                    result = __result.Where(t => t != null).Select(t => new
                    {
                        thing_id = t.thingIDNumber,
                        def_name = t.def?.defName ?? "Unknown",
                        label = t.Label,
                        nutrition = t.GetStatValue(StatDefOf.Nutrition, true)
                    }),
                    recipeDef = new
                    {
                        def_name = recipeDef.defName,
                    },
                    ticks = Find.TickManager.TicksGame
                };

                sseService.QueueEventBroadcast("make_recipe_product", payload);
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[RimworldRestApi] Error in MakeRecipeProductsPatch.Postfix: {ex}");
            }
        }
    }

    /// <summary>
    /// Patch Bill.Notify_DoBillStarted – currently just logs.
    /// </summary>
    [HarmonyPatch(typeof(Bill), nameof(Bill.Notify_DoBillStarted))]
    public static class BillStartedPatch
    {
        static void Postfix(Bill __instance, Pawn billDoer)
        {
            try
            {
                if (__instance == null || billDoer == null)
                    return;
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[RimworldRestApi] Error in BillStartedPatch.Postfix: {ex}");
            }
        }
    }

    /// <summary>
    /// Patch UnfinishedThing.BoundBill (setter) to log when a UFT is bound to a bill.
    /// </summary>
    [HarmonyPatch(typeof(UnfinishedThing), nameof(UnfinishedThing.BoundBill), MethodType.Setter)]
    public static class UnfinishedBoundPatch
    {
        static void Postfix(UnfinishedThing __instance, Bill_ProductionWithUft value)
        {
            try
            {
                if (__instance == null || value == null)
                    return;
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[RimworldRestApi] Error in UnfinishedBoundPatch.Postfix: {ex}");
            }
        }
    }

    /// <summary>
    /// Patch UnfinishedThing.Destroy to notify when an unfinished item is destroyed.
    /// This happens when work is cancelled, bill is removed, pawn dies mid-job, etc.
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

                var sse = SseService.GetService();
                if (sse == null)
                    return;

                // Bound bill (if this unfinished thing belongs to a bill)
                var bill = __instance.BoundBill;

                // IBillGiver is usually a worktable or other thing in the world
                Thing billGiverThing = bill?.billStack?.billGiver as Thing;

                var payload = new
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
                            label = billGiverThing.LabelNoCount
                        }
                        : null,
                    destroy_mode = mode.ToString(),
                    map_id = __instance.Map?.uniqueID,
                    ticks = Find.TickManager.TicksGame
                };

                sse.QueueEventBroadcast("unfinished_destroyed", payload);
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[RimworldRestApi] Error in UnfinishedDestroyPatch.Postfix: {ex}");
            }
        }
    }

    /// <summary>
    /// Sends a "date_changed" event only when in-game day, season or year changes
    /// (based on the current map's location).
    /// </summary>
    [HarmonyPatch(typeof(DateNotifier), nameof(DateNotifier.DateNotifierTick))]
    public static class DateChangePatch
    {
        // Last known date components so we can detect changes.
        private static int _lastDayOfYear = -1;
        private static Season _lastSeason = Season.Undefined;
        private static int _lastYear = -1;

        static void Postfix(DateNotifier __instance)
        {
            try
            {
                Map map = Find.CurrentMap;
                if (map == null) return;

                // Location-sensitive date: use long/lat for proper seasons.
                Vector2 longLat = Find.WorldGrid.LongLatOf(map.Tile);
                long absTicks = Find.TickManager.TicksAbs;

                // Extract components we care about.
                // DayOfYear / Year use longitude; Season uses full long/lat.
                int dayOfYear = GenDate.DayOfYear(absTicks, longLat.x);
                int year = GenDate.Year(absTicks, longLat.x);
                Season season = GenDate.Season(absTicks, longLat);

                bool dayChanged = dayOfYear != _lastDayOfYear;
                bool seasonChanged = season != _lastSeason;
                bool yearChanged = year != _lastYear;

                // No change in day, season, or year -> do nothing.
                if (!dayChanged && !seasonChanged && !yearChanged)
                    return;

                // Update cached values.
                _lastDayOfYear = dayOfYear;
                _lastSeason = season;
                _lastYear = year;

                // Human-readable date string for UI / logs.
                string fullDateWithHour = GenDate.DateReadoutStringAt(absTicks, longLat);

                var sse = SseService.GetService();
                if (sse != null)
                {
                    var payload = new
                    {
                        date = new
                        {
                            full = fullDateWithHour,
                            dayOfYear = dayOfYear,
                            year = year,
                            season = season.ToString()
                        },
                        map = new
                        {
                            id = map.uniqueID,
                            tile = map.Tile,
                            longitude = longLat.x,
                            latitude = longLat.y
                        },
                        ticksAbs = absTicks,
                        ticks = Find.TickManager.TicksGame
                    };

                    sse.QueueEventBroadcast("date_changed", payload);
                }
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[RimworldRestApi] Error in DateChangePatch.Postfix: {ex}");
            }
        }
    }
}
