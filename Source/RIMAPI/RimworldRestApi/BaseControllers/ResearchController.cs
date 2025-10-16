

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Core;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class ResearchController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public ResearchController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task GetResearchProgress(HttpListenerContext context)
        {
            try
            {
                object researchProgress = _gameDataService.GetResearchProgress();
                HandleFiltering(context, ref researchProgress);
                await ResponseBuilder.Success(context.Response, researchProgress);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetResearchFinished(HttpListenerContext context)
        {
            try
            {
                object researchFinished = _gameDataService.GetResearchFinished();
                HandleFiltering(context, ref researchFinished);
                await ResponseBuilder.Success(context.Response, researchFinished);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetResearchTree(HttpListenerContext context)
        {
            try
            {
                object researchTree = _gameDataService.GetResearchTree();
                HandleFiltering(context, ref researchTree);
                await ResponseBuilder.Success(context.Response, researchTree);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}