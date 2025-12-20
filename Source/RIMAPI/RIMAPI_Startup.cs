using Verse;
using RIMAPI.Core;

namespace RIMAPI
{
    [StaticConstructorOnStartup]
    public static class RIMAPI_Startup
    {
        static RIMAPI_Startup()
        {
            RIMAPI_GameComponent.StartServer();
        }
    }
}
