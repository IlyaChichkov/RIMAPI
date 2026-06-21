
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IColonistService
    {
        ApiResult<List<PawnDto>> GetColonists();
        ApiResult<List<PawnPositionDto>> GetColonistPositions();
        ApiResult<PawnDto> GetColonist(int pawnId);
        ApiResult<List<ApiV1PawnDetailedDto>> GetColonistsDetailedV1();
        ApiResult<ApiV1PawnDetailedDto> GetColonistDetailedV1(int pawnId);
        ApiResult<List<PawnDetailedRequestDto>> GetColonistsDetailed();
        ApiResult<PawnDetailedRequestDto> GetColonistDetailed(int pawnId);
        ApiResult<PawnInventoryDto> GetColonistInventory(int pawnId);
        ApiResult<BodyPartsDto> GetColonistBodyParts(int pawnId);
        ApiResult<OpinionAboutPawnDto> GetOpinionAboutPawn(int pawnId, int otherPawnId);
        ApiResult<WorkListDto> GetWorkList();
        ApiResult<List<TimeAssignmentDto>> GetTimeAssignmentsList();
        ApiResult SetColonistWorkPriority(WorkPriorityRequestDto body);
        ApiResult SetColonistsWorkPriority(ColonistsWorkPrioritiesRequestDto body);
        ApiResult<TraitDefDto> GetTraitDefDto(string traitName);
        ApiResult SetTimeAssignment(PawnTimeAssignmentRequestDto body);
        ApiResult MakeJobEquip(int mapId, int pawnId, int equipmentId);

        ApiResult<List<OutfitDto>> GetOutfits();
        ApiResult<ImageDto> GetPawnPortraitImage(
            int pawnId,
            int width,
            int height,
            string direction
        );
    }
}