using Verse;
using System.Threading;

namespace RIMAPI
{
    public class RIMAPI_GameComponent : GameComponent
    {
        private int tickCounter = RIMAPI_Mod.Settings.refreshIntervalTicks - 1;

        public RIMAPI_GameComponent(Game _) : base()
        {
            // void
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Server.Start();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            tickCounter++;
            if (tickCounter >= RIMAPI_Mod.Settings.refreshIntervalTicks)
            {
                tickCounter = 0;
                Server.RefreshCache();
            }
        }
    }
}
