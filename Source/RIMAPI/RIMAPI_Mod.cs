using HarmonyLib;
using RIMAPI.Core;
using UnityEngine;
using Verse;

namespace RIMAPI
{
    /// <summary>
    /// The main mod class for RIMAPI, responsible for loading settings,
    /// initializing Harmony patches, and rendering the Mod Settings UI window.
    /// </summary>
    public class RIMAPI_Mod : Mod
    {
        /// <summary>
        /// Global access to the loaded mod settings.
        /// </summary>
        public static RIMAPI_Settings Settings;

        /// <summary>
        /// Global reference to the Server-Sent Events (SSE) service.
        /// <para>This is populated when the API server starts up.</para>
        /// </summary>
        public static SseService SseService { get; private set; }

        private Harmony _harmony;

        /// <summary>
        /// Mod constructor called by RimWorld during mod loading.
        /// </summary>
        public RIMAPI_Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<RIMAPI_Settings>();
            InitializeHarmony();
        }

        /// <summary>
        /// Applies all Harmony patches defined in the assembly.
        /// </summary>
        private void InitializeHarmony()
        {
            try
            {
                _harmony = new Harmony("RIMAPI.Harmony");
                _harmony.PatchAll();
                LogApi.Info("Harmony patches applied successfully");
            }
            catch (System.Exception ex)
            {
                LogApi.Error($"Failed to apply Harmony patches - {ex}");
            }
        }

        /// <summary>
        /// Returns the name displayed in the RimWorld Mod Settings menu.
        /// </summary>
        public override string SettingsCategory() => "RIMAPI";

        /// <summary>
        /// Renders the UI content for the mod settings window.
        /// </summary>
        /// <param name="inRect">The screen rectangle available for drawing settings.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            // --- Version Information ---
            list.Label("RIMAPI.Version".Translate());
            list.Label(Settings.version.ToString());

            list.Label("RIMAPI.APIVersion".Translate());
            list.Label(Settings.apiVersion.ToString());

            list.GapLine();

            // --- Server Configuration ---

            list.Label("RIMAPI.ServerIPLabel".Translate() + " (e.g., 0.0.0.0 or localhost)");
            Settings.serverIP = Widgets.TextField(list.GetRect(24f), Settings.serverIP);

            list.Label("RIMAPI.ServerPortLabel".Translate());
            string bufferPort = Settings.serverPort.ToString();
            list.TextFieldNumeric(ref Settings.serverPort, ref bufferPort, 1, 65535);

            list.Label("RIMAPI.RefreshIntervalLabel".Translate());
            string bufferRefresh = Settings.refreshIntervalTicks.ToString();
            list.TextFieldNumeric(ref Settings.refreshIntervalTicks, ref bufferRefresh, 1);

            if (list.ButtonText("RIMAPI.RestartServer".Translate()))
            {
                RIMAPI_GameComponent.RestartServer();
            }

            if (list.ButtonText("Show Status Screen"))
            {
                // Check if window is already open to prevent duplicates
                if (!Find.WindowStack.IsOpen(typeof(RIMAPI.UI.RimApiStatusWindow)))
                {
                    Find.WindowStack.Add(new RIMAPI.UI.RimApiStatusWindow());
                }
            }

            list.GapLine();

            // --- Logging Configuration ---
            bool tempEnableLogging = Settings.EnableLogging;
            list.CheckboxLabeled("RIMAPI.EnableLogging".Translate(), ref tempEnableLogging);
            Settings.EnableLogging = tempEnableLogging;

            list.Label("RIMAPI.LoggingLevel".Translate());
            int tempLoggingLevelValue = Settings.LoggingLevel;
            string tempLoggingLevel = Settings.LoggingLevel.ToString();
            // Input range 0-4 corresponds to the enum values (Debug to Critical)
            list.TextFieldNumeric(ref tempLoggingLevelValue, ref tempLoggingLevel, 0, 4);
            Settings.LoggingLevel = tempLoggingLevelValue;

            list.GapLine();

            // --- Caching Configuration ---
            bool tempEnableCaching = Settings.EnableCaching;
            list.CheckboxLabeled("RIMAPI.EnableCaching".Translate(), ref tempEnableCaching);
            Settings.EnableCaching = tempEnableCaching;

            bool tempCacheLogStatistics = Settings.CacheLogStatistics;
            list.CheckboxLabeled("RIMAPI.CacheLogStatistics".Translate(), ref tempCacheLogStatistics);
            Settings.CacheLogStatistics = tempCacheLogStatistics;

            list.Label("RIMAPI.CacheDefaultExpirationSeconds".Translate());
            string bufferCacheDefaultExpirationSeconds = Settings.CacheDefaultExpirationSeconds.ToString();
            list.TextFieldNumeric(
                ref Settings.CacheDefaultExpirationSeconds,
                ref bufferCacheDefaultExpirationSeconds,
                60 // Minimum 60 seconds
            );

            list.End();
        }

        /// <summary>
        /// Registers the active SSE service instance for global access.
        /// Called by RIMAPI_GameComponent when the server starts.
        /// </summary>
        public static void RegisterSseService(SseService sseService)
        {
            SseService = sseService;
        }
    }
}