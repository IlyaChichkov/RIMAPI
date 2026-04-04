using System.Linq;
using RimWorld;
using Verse;

namespace RIMAPI.Helpers
{
    public static class FactionHelper
    {
        public static Faction GetFactionByOrderId(int id)
        {
            return Find.FactionManager.AllFactionsListForReading.FirstOrDefault(s =>
                s.loadID == id
            );
        }

        public static FactionDef GetFactionDef(string defName)
        {
            return DefDatabase<FactionDef>.GetNamed(defName);
        }
    }
}
