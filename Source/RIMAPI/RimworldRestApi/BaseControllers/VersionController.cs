using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Models;
using RimworldRestApi.Core;

namespace RimworldRestApi.Controllers
{
    public class VersionController : BaseController
    {
        public async Task GetVersion(HttpListenerContext context)
        {
            var versionInfo = new VersionDto
            {
                Version = "1.0.0",
                RimWorldVersion = "1.5.4104",
                ModVersion = "1.0.0",
                ApiVersion = "v1"
            };

            await ResponseBuilder.Success(context.Response, versionInfo);
        }
    }
}