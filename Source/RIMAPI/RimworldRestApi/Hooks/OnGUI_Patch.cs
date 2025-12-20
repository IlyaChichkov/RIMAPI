using HarmonyLib;
using Verse;

namespace RIMAPI.Hooks
{
    [HarmonyPatch(typeof(Root), nameof(Root.OnGUI))]
    public static class OnGUI_Patch
    {
        public static void Postfix()
        {
            RIMAPI_GameComponent.ProcessServerQueues();
        }
    }
}
