using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI
{
    public class RIMAPI_Settings : ModSettings
    {
        public string version = "0.4.4";
        public string apiVersion = "v1";
        public int serverPort = 8765;
        public int refreshIntervalTicks = 300;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref serverPort, "serverPort", 8765);
            Scribe_Values.Look(ref refreshIntervalTicks, "refreshIntervalTicks", 300);
        }
    }
}
