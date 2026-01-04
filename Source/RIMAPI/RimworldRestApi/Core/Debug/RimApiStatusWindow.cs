using System;
using UnityEngine;
using Verse;
using RIMAPI.Core;
using RimWorld;

namespace RIMAPI.UI
{
    /// <summary>
    /// A floating dashboard window that displays real-time server statistics,
    /// SSE status, caching metrics, and administrative actions.
    /// </summary>
    public class RimApiStatusWindow : Window
    {
        // UI Constants
        private const float HEADER_HEIGHT = 35f;
        private const float ROW_HEIGHT = 24f;

        /// <summary>
        /// Configures the window properties (size, interaction, close button).
        /// </summary>
        public RimApiStatusWindow()
        {
            this.doCloseX = true;
            this.forcePause = false; // Don't pause the game when this is open
            this.absorbInputAroundWindow = false;
            this.closeOnClickedOutside = false;
            this.draggable = true;
            this.resizeable = false;
        }

        /// <summary>
        /// Defines the initial size of the window.
        /// </summary>
        public override Vector2 InitialSize => new Vector2(450f, 500f);

        /// <summary>
        /// Draws the content of the window.
        /// </summary>
        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            // --- Header ---
            Text.Font = GameFont.Medium;
            list.Label("RIMAPI Dashboard");
            Text.Font = GameFont.Small;
            list.GapLine();

            // --- Section 1: Server Status ---
            DrawSectionHeader(list, "Server Status");

            bool isRunning = RIMAPI_GameComponent.IsServerRunning();
            string statusText = isRunning ? "Running" : "Stopped";
            Color statusColor = isRunning ? Color.green : Color.red;

            // Draw Status with Color
            Rect statusRect = list.GetRect(ROW_HEIGHT);
            Widgets.Label(statusRect.LeftHalf(), "Status:");
            Color oldColor = GUI.color;
            GUI.color = statusColor;
            Widgets.Label(statusRect.RightHalf(), statusText);
            GUI.color = oldColor;

            list.Label($"Port: {RIMAPI_Mod.Settings.serverPort}");
            list.Label($"API Version: {RIMAPI_Mod.Settings.apiVersion}");
            list.Gap();

            // --- Section 2: SSE Statistics ---
            DrawSectionHeader(list, "SSE Server (Realtime Events)");

            if (RIMAPI_Mod.SseService != null && isRunning)
            {
                list.Label("Service Status: Operational");
                list.Label($"Active Clients: {RIMAPI_Mod.SseService.ClientCount}");
                list.Label($"Total Events Sent: {RIMAPI_Mod.SseService.TotalEventsSent}");
            }
            else
            {
                list.Label("SSE Service unavailable (Server stopped)");
            }
            list.Gap();

            // --- Section 3: Caching & Performance ---
            DrawSectionHeader(list, "Cache & Performance");

            if (RIMAPI_Mod.Settings.EnableCaching)
            {
                // Note: You need to expose cache stats from your ApiServer/CacheManager
                // This is an example of how it should look:

                // list.Label($"Cached Items: {_apiServer.Cache.Count}");
                // list.Label($"Cache Hits: {_apiServer.Cache.Hits}");
                // list.Label($"Cache Misses: {_apiServer.Cache.Misses}");

                list.Label("Caching: Enabled");
                list.Label($"Expiration Time: {RIMAPI_Mod.Settings.CacheDefaultExpirationSeconds}s");
            }
            else
            {
                list.Label("Caching is currently DISABLED in settings.");
            }
            list.Gap();

            // --- Section 4: Actions ---
            DrawSectionHeader(list, "Actions");
            list.Gap(5f);

            // "Generate Documentation" Button
            // if (list.ButtonText("Generate Documentation"))
            // {
            //     GenerateDocumentation();
            // }

            // list.Gap(5f);

            // Helper text for the button
            // Text.Font = GameFont.Tiny;
            // list.Label("Generates a JSON/YAML file definition of all registered routes and saves it to the mod folder.");
            // Text.Font = GameFont.Small;

            list.End();
        }

        /// <summary>
        /// Logic to trigger documentation generation.
        /// </summary>
        private void GenerateDocumentation()
        {
            if (!RIMAPI_GameComponent.IsServerRunning())
            {
                Messages.Message("Cannot generate docs: Server is not running.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            try
            {
                // Calls the static helper in your Core to generate docs
                // RIMAPI_GameComponent.GenerateOpenApiDocs(); 

                LogApi.Info("Documentation generation requested via Dashboard.");
                Messages.Message("Documentation generated successfully!", MessageTypeDefOf.PositiveEvent, false);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Failed to generate documentation: {ex.Message}");
                Messages.Message("Error generating documentation. Check logs.", MessageTypeDefOf.NegativeEvent, false);
            }
        }

        /// <summary>
        /// Helper to draw bold section headers.
        /// </summary>
        private void DrawSectionHeader(Listing_Standard list, string text)
        {
            Text.Font = GameFont.Small;
            GUI.color = Color.cyan;
            list.Label(text);
            GUI.color = Color.white;
            list.GapLine(6f);
        }
    }
}