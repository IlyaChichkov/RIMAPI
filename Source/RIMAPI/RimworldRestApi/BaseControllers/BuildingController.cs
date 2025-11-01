using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Core;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class BuildingController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public BuildingController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task GetBuildingInfo(HttpListenerContext context)
        {
            try
            {
                var buildingId = GetIntProperty(context);
                object buildingTurret = _gameDataService.GetBuildingInfo(buildingId);
                HandleFiltering(context, ref buildingTurret);
                await ResponseBuilder.Success(context.Response, buildingTurret);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}