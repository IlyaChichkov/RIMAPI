using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Models;
using RimworldRestApi.Core;
using Verse;
using RimWorld;

namespace RimworldRestApi.Controllers
{
    public class VersionController : BaseController
    {
        public async Task GetVersion(HttpListenerContext context)
        {
            Log.Message("RIMAPI: VersionController.GetVersion called");

            try
            {
                var versionInfo = new VersionDto
                {
                    Version = "1.0.0",
                    RimWorldVersion = VersionControl.CurrentVersionString,
                    ModVersion = "1.0.0",
                    ApiVersion = "v1"
                };

                Log.Message($"RIMAPI: Returning version info: {versionInfo.ModVersion}");
                await ResponseBuilder.Success(context.Response, versionInfo);
            }
            catch (System.Exception ex)
            {
                Log.Error($"RIMAPI: Error in VersionController: {ex}");
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, "Internal server error");
            }
        }
    }
}