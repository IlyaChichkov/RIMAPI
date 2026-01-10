using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimworldRestApi.Hooks
{
    /// <summary>
    /// Patches for map events: Mining rocks, Harvesting plants, Chopping trees.
    /// </summary>
    public static class MapEventsHook
    {
        /// <summary>
        /// Hook into Mineable.DestroyMined to detect when a rock/ore is successfully mined.
        /// </summary>
        [HarmonyPatch(typeof(Mineable), nameof(Mineable.DestroyMined))]
        public static class MineableDestroyMinedPatch
        {
            static void Postfix(Mineable __instance, Pawn pawn)
            {
                try
                {
                    if (__instance == null || pawn == null) return;

                    EventPublisherAccess.Publish(
                        "rock_mined",
                        new
                        {
                            pawn = new
                            {
                                id = pawn.thingIDNumber,
                                name = pawn.Name?.ToStringShort ?? "Unknown"
                            },
                            target = new
                            {
                                id = __instance.thingIDNumber,
                                def_name = __instance.def?.defName ?? "Unknown",
                                label = __instance.Label,
                                position = new { x = __instance.Position.x, z = __instance.Position.z }
                            },
                            map_id = __instance.Map?.uniqueID ?? -1,
                            ticks = Find.TickManager.TicksGame
                        }
                    );
                }
                catch (Exception ex)
                {
                    RIMAPI.Core.LogApi.Error($"[RimworldRestApi] Error in MineableDestroyMinedPatch: {ex}");
                }
            }
        }

        /// <summary>
        /// Hook into Plant.PlantCollected to detect when a crop or wild plant is harvested for resources.
        /// </summary>
        [HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
        public static class PlantCollectedPatch
        {
            static void Postfix(Plant __instance, Pawn by)
            {
                try
                {
                    if (__instance == null || by == null) return;

                    EventPublisherAccess.Publish(
                        "plant_harvested",
                        new
                        {
                            pawn = new
                            {
                                id = by.thingIDNumber,
                                name = by.Name?.ToStringShort ?? "Unknown"
                            },
                            target = new
                            {
                                id = __instance.thingIDNumber,
                                def_name = __instance.def?.defName ?? "Unknown",
                                label = __instance.Label,
                                position = new { x = __instance.Position.x, z = __instance.Position.z },
                                growth = __instance.Growth
                            },
                            yield_now = __instance.YieldNow(),
                            map_id = __instance.Map?.uniqueID ?? -1,
                            ticks = Find.TickManager.TicksGame
                        }
                    );
                }
                catch (Exception ex)
                {
                    RIMAPI.Core.LogApi.Error($"[RimworldRestApi] Error in PlantCollectedPatch: {ex}");
                }
            }
        }

        /// <summary>
        /// Hook into JobDriver.EndJobWith to detect when a PlantCut job (chopping trees) succeeds.
        /// There isn't a direct "OnTreeChopped" method, so we catch the successful end of the chop job.
        /// </summary>
        [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.EndJobWith))]
        public static class JobDriverEndPatch
        {
            static void Postfix(JobDriver __instance, JobCondition condition)
            {
                try
                {
                    // Filter for successful PlantCut jobs only
                    if (condition != JobCondition.Succeeded) return;
                    if (!(__instance is JobDriver_PlantCut)) return;

                    Pawn pawn = __instance.pawn;
                    Thing plant = __instance.job?.targetA.Thing;

                    if (pawn == null || plant == null) return;

                    EventPublisherAccess.Publish(
                        "plant_cut",
                        new
                        {
                            pawn = new
                            {
                                id = pawn.thingIDNumber,
                                name = pawn.Name?.ToStringShort ?? "Unknown"
                            },
                            target = new
                            {
                                id = plant.thingIDNumber,
                                def_name = plant.def?.defName ?? "Unknown",
                                label = plant.Label,
                                is_tree = plant.def?.plant?.IsTree ?? false,
                                position = new { x = plant.Position.x, z = plant.Position.z }
                            },
                            map_id = pawn.Map?.uniqueID ?? -1,
                            ticks = Find.TickManager.TicksGame
                        }
                    );
                }
                catch (Exception ex)
                {
                    RIMAPI.Core.LogApi.Error($"[RimworldRestApi] Error in JobDriverEndPatch (PlantCut): {ex}");
                }
            }
        }
    }
}