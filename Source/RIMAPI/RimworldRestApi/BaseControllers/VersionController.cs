using System;
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
        public async Task GetVersion(HttpListenerContext context, RIMAPI.RIMAPI_Settings Settings)
        {
            try
            {
                object version = new VersionDto
                {
                    RimWorldVersion = VersionControl.CurrentVersionString,
                    ModVersion = Settings.version,
                    ApiVersion = Settings.apiVersion
                };

                HandleFiltering(context, ref version);
                await ResponseBuilder.Success(context.Response, version);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}