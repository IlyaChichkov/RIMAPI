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
        public async Task GetVersion(HttpListenerContext context)
        {
            try
            {
                // TODO: Pull the data from config
                object version = new VersionDto
                {
                    Version = "1.0.0",
                    RimWorldVersion = VersionControl.CurrentVersionString,
                    ModVersion = "1.0.0",
                    ApiVersion = "v1"
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