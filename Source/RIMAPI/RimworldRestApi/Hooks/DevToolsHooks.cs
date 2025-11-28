using System;
using HarmonyLib;
using Verse;

namespace RimworldRestApi.Hooks
{
    /// <summary>
    /// Hooks RimWorld dev console messages (Log.Message/Warning/Error) and
    /// forwards them as SSE events so external tools can see the log stream.
    /// </summary>
    public static class DevConsoleLogHooks
    {
        private static void SendLogToSse(string level, string text)
        {
            try
            {
                // Prevent recurse logging
                if (
                    text.Contains("log_message")
                    || text.Contains("[SSE]")
                    || text.ToLower().Contains("sse")
                )
                {
                    return;
                }

                var payload = new
                {
                    level = level, // "Info" | "Warning" | "Error"
                    message = text,
                    ticks = Find.TickManager?.TicksGame ?? 0,
                };

                EventPublisherAccess.Publish("log_message", payload);
            }
            catch
            {
                // Important: do NOT Log.Message here – that would recurse into this patch.
                // Swallow errors; log hooks must never crash the game.
            }
        }

        /// <summary>
        /// Hook for Log.Message(string).
        /// </summary>
        [HarmonyPatch(typeof(Log), nameof(Log.Message), new Type[] { typeof(string) })]
        public static class LogMessagePatch
        {
            static void Postfix(string text)
            {
                SendLogToSse("Info", text);
            }
        }

        /// <summary>
        /// Hook for Log.Warning(string).
        /// </summary>
        [HarmonyPatch(typeof(Log), nameof(Log.Warning), new Type[] { typeof(string) })]
        public static class LogWarningPatch
        {
            static void Postfix(string text)
            {
                SendLogToSse("Warning", text);
            }
        }

        /// <summary>
        /// Hook for Log.Error(string).
        /// </summary>
        [HarmonyPatch(typeof(Log), nameof(Log.Error), new Type[] { typeof(string) })]
        public static class LogErrorPatch
        {
            static void Postfix(string text)
            {
                SendLogToSse("Error", text);
            }
        }
    }
}
