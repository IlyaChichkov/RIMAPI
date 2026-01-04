using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IGlobalMapService
    {
        ApiResult<List<SettlementDto>> GetSettlements();
        ApiResult<List<SettlementDto>> GetPlayerSettlements();
        ApiResult<List<CaravanDto>> GetCaravans();
        ApiResult<List<SiteDto>> GetSites();
        ApiResult<TileDto> GetTile(int tileId);
        ApiResult<List<TileDto>> GetTilesInRadius(int tileId, float radius);
        ApiResult<CoordinatesDto> GetTileCoordinates(int tileId);
    }
}