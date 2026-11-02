using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class ResearchController
    {
        private readonly IResearchService _researchService;

        public ResearchController(IResearchService researchService)
        {
            _researchService = researchService;
        }

        [Get("/api/v1/research/progress")]
        public async Task GetResearchProgress(RimApiContext context)
        {
            var result = _researchService.GetResearchProgress();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/research/finished")]
        public async Task GetResearchFinished(RimApiContext context)
        {
            var result = _researchService.GetResearchFinished();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/research/tree")]
        public async Task GetResearchTree(RimApiContext context)
        {
            var result = _researchService.GetResearchTree();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/research/project")]
        public async Task GetResearchProjectByName(RimApiContext context)
        {
            var name = RequestParser.GetStringParameter(context, "name");
            var result = _researchService.GetResearchProjectByName(name);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/research/summary")]
        public async Task GetResearchSummary(RimApiContext context)
        {
            var result = _researchService.GetResearchSummary();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/research/target")]
        [EndpointMetadata("Set the current research project by defName")]
        public async Task SetResearchTarget(RimApiContext context)
        {
            var name = RequestParser.GetStringParameter(context, "name");
            var force = RequestParser.GetBooleanParameter(context, "force");
            var result = _researchService.SetResearchTarget(name, force);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/research/stop")]
        [EndpointMetadata("Stop the currently active research project")]
        public async Task StopCurrentProject(RimApiContext context)
        {
            var result = _researchService.StopCurrentProject();
            await context.SendJsonResponse(result);
        }
    }
}
