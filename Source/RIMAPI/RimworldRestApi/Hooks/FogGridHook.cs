using System;
using HarmonyLib;
using Verse;

namespace RimworldRestApi.Hooks
{
    /// <summary>
    /// Patches map fog updates (Area Revealed events).
    /// </summary>
    public static class FogGridHook
    {
        /// <summary>
        /// Hook into FloodFillerFog.FloodUnfog. 
        /// This is the engine method that calculates and updates the visibility grid 
        /// when walls are mined, doors opened, or areas discovered.
        /// </summary>
        [HarmonyPatch(typeof(FloodFillerFog), nameof(FloodFillerFog.FloodUnfog))]
        public static class FloodUnfogPatch
        {
            static void Postfix(IntVec3 root, Map map, FloodUnfogResult __result)
            {
                try
                {
                    // If no cells were actually unfogged, we usually don't care (performance check)
                    if (__result.cellsUnfogged == 0) return;
                    if (map == null) return;

                    EventPublisherAccess.Publish(
                        "fog_updated",
                        new
                        {
                            map_id = map.uniqueID,
                            root_cell = new { x = root.x, z = root.z },
                            cells_revealed = __result.cellsUnfogged,
                            mechanoid_found = __result.mechanoidFound,
                            all_on_screen = __result.allOnScreen,
                            ticks = Find.TickManager.TicksGame
                        }
                    );
                }
                catch (Exception ex)
                {
                    RIMAPI.Core.LogApi.Error($"[RimworldRestApi] Error in FloodUnfogPatch: {ex}");
                }
            }
        }
    }
}