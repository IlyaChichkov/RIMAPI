using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IPawnInfoService
    {
        // Get all pawns (enemies, animals, colonists) on a map
        ApiResult<List<ColonistDto>> GetPawnsOnMap(int mapId);

        // Get aggregated details
        ApiResult<ColonistDetailedDto> GetPawnDetails(int pawnId);

        // Specific data endpoints
        ApiResult<ColonistInventoryDto> GetPawnInventory(int pawnId);
    }
}