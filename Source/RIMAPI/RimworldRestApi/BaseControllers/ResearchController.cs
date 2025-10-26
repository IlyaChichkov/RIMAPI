

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

        public async Task GetResearchProject(HttpListenerContext context)
        {
            try
            {
                string projectNameStr = context.Request.QueryString["name"];
                if (string.IsNullOrEmpty(projectNameStr))
                {
                    throw new Exception("Missing project def name parameter");
                }

                object project = _gameDataService.GetResearchProjectByName(projectNameStr);
                HandleFiltering(context, ref project);
                await ResponseBuilder.Success(context.Response, project);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetResearchSummary(HttpListenerContext context)
        {
            try
            {
                object researchSummary = _gameDataService.GetResearchSummary();
                HandleFiltering(context, ref researchSummary);
                await ResponseBuilder.Success(context.Response, researchSummary);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}