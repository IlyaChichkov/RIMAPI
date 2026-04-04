using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IFactionService
    {
        ApiResult<List<FactionsDto>> GetFactions();
        ApiResult<FactionDto> GetFaction(int id);
        ApiResult<FactionDto> GetPlayerFaction();
        ApiResult<FactionRelationDto> GetFactionRelationWith(int id, int otherId);
        ApiResult<FactionRelationsDto> GetFactionRelations(int id);
        ApiResult<FactionDefDto> GetFactionDef(string defName);
        ApiResult<FactionChangeRelationResponceDto> ChangeFactionRelationWith(
            int id,
            int otherId,
            int change,
            bool sendMessage,
            bool canSendHostilityLetter
        );
        ApiResult<FactionChangeRelationResponceDto> SetFactionGoodwill(
            int id,
            int otherId,
            int goodwill,
            bool sendMessage,
            bool canSendHostilityLetter
        );
        ApiResult<FactionIconImageDto> GetFactionIcon(int id);
    }
}