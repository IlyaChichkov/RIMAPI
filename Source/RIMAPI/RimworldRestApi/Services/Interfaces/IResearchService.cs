
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IResearchService
    {
        ApiResult<ResearchProjectDto> GetResearchProgress();
        ApiResult<ResearchFinishedDto> GetResearchFinished();
        ApiResult<ResearchTreeDto> GetResearchTree();
        ApiResult<ResearchProjectDto> GetResearchProjectByName(string name);
        ApiResult<ResearchSummaryDto> GetResearchSummary();
    }
}
