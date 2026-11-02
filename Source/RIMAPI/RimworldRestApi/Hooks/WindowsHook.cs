using System;
using HarmonyLib;
using RIMAPI.Core;
using Verse;

namespace RimworldRestApi.Hooks
{
    [HarmonyPatch(typeof(DiaOption), "Activate")]
    public static class DialogOptionPatch
    {
        /// <summary>
        /// Capture the dialog text before Activate moves to the next node or closes the dialog.
        /// </summary>
        static void Prefix(DiaOption __instance, out string __state)
        {
            __state = null;
            try
            {
                if (__instance?.dialog is Dialog_NodeTree nodeTree)
                {
                    // Access private 'curNode'
                    DiaNode currentNode = Traverse.Create(nodeTree).Field("curNode").GetValue<DiaNode>();
                    __state = currentNode?.text;
                }
            }
            catch (Exception ex)
            {
                LogApi.Error($"[RimworldRestApi] Error in DialogOptionPatch.Prefix: {ex}");
            }
        }

        static void Postfix(DiaOption __instance, string __state)
        {
            try
            {
                if (__instance == null)
                    return;

                string selectedOptionLabel = Traverse.Create(__instance).Field("text").GetValue<string>();

                // The option's own dialog; the window stack may already be closed or topped by
                // an always-on-top ImmediateWindow such as the learning helper
                Window ownerWindow = __instance.dialog;
                string windowType = ownerWindow?.GetType().Name ?? "Unknown";
                if (windowType.Contains("Debug") || windowType.Contains("OptionListLister"))
                {
                    return;
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
                            text = __state ?? "Unknown",
                            window_type = windowType
                        },
                        ticks = Current.Game?.tickManager?.TicksGame ?? 0
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
