using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using Verse;

namespace RIMAPI.Services
{
    public class ResearchService : IResearchService
    {
        public ResearchService() { }

        public ApiResult<ResearchFinishedDto> GetResearchFinished()
        {
            var result = ResearchHelper.GetResearchFinished();
            return ApiResult<ResearchFinishedDto>.Ok(result);
        }

        public ApiResult<ResearchProjectDto> GetResearchProgress()
        {
            var result = ResearchHelper.GetResearchProgress();
            if (result == null)
            {
                return ApiResult<ResearchProjectDto>
                    .Ok(new ResearchProjectDto
                    {
                        Name = "none",
                        Label = "None",
                        Description = "None"
                    });
            }
            return ApiResult<ResearchProjectDto>.Ok(result);
        }

        public ApiResult<ResearchProjectDto> GetResearchProjectByName(string name)
        {
            var result = ResearchHelper.GetResearchProjectByName(name);
            if (result == null)
            {
                return ApiResult<ResearchProjectDto>
                    .Fail($"Couldn't find research project with name: {name}");
            }
            return ApiResult<ResearchProjectDto>.Ok(result);
        }

        public ApiResult<ResearchSummaryDto> GetResearchSummary()
        {
            var result = ResearchHelper.GetResearchSummary();
            return ApiResult<ResearchSummaryDto>.Ok(result);
        }

        public ApiResult<ResearchTreeDto> GetResearchTree()
        {
            var result = ResearchHelper.GetResearchTree();
            return ApiResult<ResearchTreeDto>.Ok(result);
        }
    }
}
