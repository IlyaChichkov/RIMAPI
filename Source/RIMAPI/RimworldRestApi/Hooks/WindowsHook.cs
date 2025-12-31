using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RIMAPI.Core;
using Verse;

namespace RimworldRestApi.Hooks
{
    [HarmonyPatch(typeof(DiaOption), "Activate")]
    public static class DialogOptionPatch
    {
        static void Postfix(DiaOption __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                string selectedOptionLabel = Traverse.Create(__instance).Field("text").GetValue<string>();

                string dialogContext = "Unknown";

                Window activeWindow = null;
                if (Find.WindowStack.Windows.Count > 0)
                {
                    activeWindow = Find.WindowStack.Windows.Last();
                }

                if (activeWindow is Dialog_NodeTree nodeTree)
                {
                    // Access private 'curNode'
                    DiaNode currentNode = Traverse.Create(nodeTree).Field("curNode").GetValue<DiaNode>();
                    if (currentNode != null)
                    {
                        dialogContext = currentNode.text;
                    }
                }
                else
                {
                    string windowType = activeWindow?.GetType().Name ?? "null";
                    if (windowType.Contains("Debug") || windowType.Contains("OptionListLister"))
                    {
                        return;
                    }
                }

                EventPublisherAccess.Publish(
                    "dialog_option_selected",
                    new
                    {
                        option = new
                        {
                            label = selectedOptionLabel,
                            resolve_tree = __instance.resolveTree,
                            disabled = __instance.disabled,
                        },
                        dialog = new
                        {
                            text = dialogContext,
                            window_type = activeWindow?.GetType().Name ?? "Unknown"
                        },
                        ticks = Find.TickManager.TicksGame
                    }
                );

                LogApi.Info($"[RimworldRestApi] Dialog Option Selected: {selectedOptionLabel}");
            }
            catch (Exception ex)
            {
                LogApi.Error($"[RimworldRestApi] Error in DialogOptionPatch.Postfix: {ex}");
            }
        }
    }
}