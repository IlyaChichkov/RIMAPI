using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class ResearchController : RequestParser
    {
        private readonly IResearchService _researchService;

        public ResearchController(IResearchService researchService)
        {
            _researchService = researchService;
        }

        [Get("/api/v1/research/progress")]
        public async Task GetResearchProgress(HttpListenerContext context)
        {
            var result = _researchService.GetResearchProgress();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/research/finished")]
        public async Task GetResearchFinished(HttpListenerContext context)
        {
            var result = _researchService.GetResearchFinished();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/research/tree")]
        public async Task GetResearchTree(HttpListenerContext context)
        {
            var result = _researchService.GetResearchTree();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/research/project")]
        public async Task GetResearchProjectByName(HttpListenerContext context)
        {
            var name = GetStringParameter(context, "name");
            var result = _researchService.GetResearchProjectByName(name);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/research/summary")]
        public async Task GetResearchSummary(HttpListenerContext context)
        {
            var result = _researchService.GetResearchSummary();
            await context.SendJsonResponse(result);
        }
    }
}
