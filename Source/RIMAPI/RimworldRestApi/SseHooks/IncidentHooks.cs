

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimworldRestApi.Core;
using RimworldRestApi.SseModels;
using Verse;

namespace RimworldRestApi.Hooks
{

    [HarmonyPatch]
    public static class AllLettersPatch
    {
        /// <summary>
        /// Dynamically find the ReceiveLetter overload that takes a Letter as its first argument.
        /// This is safer across RimWorld versions than hardcoding the full parameter list.
        /// </summary>
        static MethodBase TargetMethod()
        {
            return AccessTools
                .GetDeclaredMethods(typeof(LetterStack))
                .FirstOrDefault(m =>
                {
                    if (m.Name != "ReceiveLetter") return false;
                    var p = m.GetParameters();
                    return p.Length > 0 && p[0].ParameterType == typeof(Letter);
                });
        }

        static void Postfix(Letter let)
        {
            try
            {
                if (let == null)
                    return;

                var sse = SseService.GetService();
                if (sse == null)
                    return;

                string label = let.Label;

                var payload = new
                {
                    letter = new
                    {
                        label = label,
                        def = let.def?.defName,
                        className = let.GetType().Name,
                    },
                    targets = ExtractLookTargetsLite(let.lookTargets),
                    ticks = Find.TickManager.TicksGame
                };

                sse.QueueEventBroadcast("letter_received", payload);
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[RimworldRestApi] Error in AllLettersPatch.Postfix: {ex}");
            }
        }

        private static object ExtractLookTargetsLite(LookTargets lookTargets)
        {
            if (lookTargets == null)
                return null;

            var primary = lookTargets.PrimaryTarget;
            string primaryLabel = null;
            string primaryDefName = null;
            string primaryPos = null;
            int? primaryMapId = null;

            if (primary.Thing != null)
            {
                primaryLabel = primary.Thing.LabelCap;
                primaryDefName = primary.Thing.def?.defName;
                primaryPos = primary.Thing.Position.ToString();
                primaryMapId = primary.Thing.Map?.uniqueID;
            }
            else if (primary.Cell.IsValid)
            {
                primaryPos = primary.Cell.ToString();
            }

            return new
            {
                primaryLabel,
                primaryDefName,
                primaryPos,
                primaryMapId
            };
        }
    }

    /// <summary>
    /// Catch-all patch for any in-game message pushed via Messages.Message(Message,bool).
    /// This covers the majority of pop-up messages, including those created by other mods, 
    /// because the other overloads eventually funnel into this one.
    /// </summary>
    [HarmonyPatch(typeof(Messages), nameof(Messages.Message), new Type[] { typeof(Message), typeof(bool) })]
    public static class AllMessagesPatch
    {
        static void Postfix(Message msg, bool historical)
        {
            try
            {
                if (msg == null)
                    return;

                var sse = SseService.GetService();
                if (sse == null)
                    return;

                var payload = new
                {
                    message = new
                    {
                        text = msg.text ?? string.Empty,
                        def = msg.def?.defName,               // MessageTypeDef (e.g. "ThreatBig", "PositiveEvent")
                        // whether this was stored in the message history or not
                        historical = historical,
                    },
                    targets = ExtractLookTargetsLite(msg.lookTargets),
                    ticks = Find.TickManager.TicksGame
                };

                sse.QueueEventBroadcast("message_received", payload);

                DebugLogging.Info($"Message received: {msg.text}");
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[RimworldRestApi] Error in AllMessagesPatch.Postfix: {ex}");
            }
        }

        private static object ExtractLookTargetsLite(LookTargets lookTargets)
        {
            if (lookTargets == null)
                return null;

            var primary = lookTargets.PrimaryTarget;
            string primaryLabel = null;
            string primaryDefName = null;
            string primaryPos = null;
            int? primaryMapId = null;

            if (primary.Thing != null)
            {
                primaryLabel = primary.Thing.LabelCap;
                primaryDefName = primary.Thing.def?.defName;
                primaryPos = primary.Thing.Position.ToString();
                primaryMapId = primary.Thing.Map?.uniqueID;
            }
            else if (primary.Cell.IsValid)
            {
                primaryPos = primary.Cell.ToString();
            }

            return new
            {
                primaryLabel,
                primaryDefName,
                primaryPos,
                primaryMapId
            };
        }
    }
}