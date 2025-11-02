using HarmonyLib;
using RimWorld;
using RimworldRestApi.Core;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimworldRestApi.Hooks
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
    public static class FinalizeIngest_Patch
    {
        static void Postfix(Thing __instance, Pawn ingester, float nutritionWanted, float __result)
        {
            try
            {
                if (ingester?.IsColonist == true)
                {
                    var sseService = GetSseService();
                    sseService?.BroadcastFoodEvent(ingester, __instance, nutritionWanted, __result);
                }
            }
            catch (System.Exception ex)
            {
                DebugLogging.Error($"Error in Ingested patch - {ex}");
            }
        }

        private static SseService GetSseService()
        {
            return RIMAPI.RIMAPI_Mod.SseService;
        }
    }
}