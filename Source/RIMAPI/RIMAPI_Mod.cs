using HarmonyLib;
using RimworldRestApi.Core;
using UnityEngine;
using Verse;

namespace RIMAPI
{
    public class RIMAPI_Mod : Mod
    {
        public static RIMAPI_Settings Settings;
        public static SseService SseService { get; private set; }
        private Harmony _harmony;

        public RIMAPI_Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<RIMAPI_Settings>();
            InitializeHarmony();
        }

        private void InitializeHarmony()
        {
            try
            {
                _harmony = new Harmony("RIMAPI.Harmony");
                _harmony.PatchAll();
                Log.Message("RIMAPI: Harmony patches applied successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"RIMAPI: Failed to apply Harmony patches - {ex}");
            }
        }

        public override string SettingsCategory() => "RIMAPI";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            list.Label("Version".Translate());
            list.Label(Settings.version.ToString());

            list.Label("API Version".Translate());
            list.Label(Settings.apiVersion.ToString());

            list.Label("RIMAPI.ServerPortLabel".Translate());
            string bufferPort = Settings.serverPort.ToString();
            list.TextFieldNumeric(ref Settings.serverPort, ref bufferPort, 1, 65535);

            list.Label("RIMAPI.RefreshIntervalLabel".Translate());
            string bufferRefresh = Settings.refreshIntervalTicks.ToString();
            list.TextFieldNumeric(ref Settings.refreshIntervalTicks, ref bufferRefresh, 1);

            list.End();
        }

        public static void RegisterSseService(SseService sseService)
        {
            SseService = sseService;
        }
    }
}
