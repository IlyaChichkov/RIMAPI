using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface ILearningService
    {
        ApiResult<List<string>> GetLearningConceptsDefs();
        ApiResult<List<LearningConceptDto>> GetAllConcepts();
        ApiResult<List<LearningConceptDto>> GetActiveConcepts();
        ApiResult<LearningConceptDto> GetConceptByDef(string defName);
        ApiResult<bool> MarkConceptLearned(LearningConceptMarkDto request);
    }
}