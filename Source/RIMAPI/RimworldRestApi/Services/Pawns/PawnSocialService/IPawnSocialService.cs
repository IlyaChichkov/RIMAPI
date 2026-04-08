using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IPawnSocialService
    {
        ApiResult<List<InteractionDefDto>> GetInteractionDefs();
        ApiResult<PawnInteractionStatusDto> GetPawnInteractionStatus(int pawnId);
        ApiResult<PawnInteractionLogDto> GetPawnInteractionLog(int pawnId);
        ApiResult<PawnRelationsDto> GetPawnRelations(int pawnId);
        ApiResult<List<PawnOpinionDto>> GetPawnOpinions(int pawnId);
        ApiResult ForceInteraction(ForceInteractionRequestDto request);
        ApiResult AddRelation(AddRelationRequestDto request);
        ApiResult RemoveRelation(RemoveRelationRequestDto request);
    }
}
