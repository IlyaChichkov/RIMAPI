using UnityEngine;
using Verse;

namespace RIMAPI
{
    public class RIMAPI_Mod : Mod
    {
        public static RIMAPI_Settings Settings;

        public RIMAPI_Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<RIMAPI_Settings>();
        }

        public override string SettingsCategory() => "RIMAPI";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            list.Label("RIMAPI.ServerPortLabel".Translate());
            string bufferPort = Settings.serverPort.ToString();
            list.TextFieldNumeric(ref Settings.serverPort, ref bufferPort, 1, 65535);

            list.Label("RIMAPI.RefreshIntervalLabel".Translate());
            string bufferRefresh = Settings.refreshIntervalTicks.ToString();
            list.TextFieldNumeric(ref Settings.refreshIntervalTicks, ref bufferRefresh, 1);

            list.End();
        }
    }
}
