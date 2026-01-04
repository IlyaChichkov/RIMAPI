using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public class GlobalMapService : IGlobalMapService
    {
        public ApiResult<List<SettlementDto>> GetSettlements()
        {
            var result = GlobalMapHelper.GetSettlements();
            return ApiResult<List<SettlementDto>>.Ok(result);
        }

        public ApiResult<List<SettlementDto>> GetPlayerSettlements()
        {
            var result = GlobalMapHelper.GetPlayerSettlements();
            return ApiResult<List<SettlementDto>>.Ok(result);
        }


        public ApiResult<List<CaravanDto>> GetCaravans()
        {
            var result = CaravanHelper.GetCaravans();
            return ApiResult<List<CaravanDto>>.Ok(result);
        }

        public ApiResult<List<SiteDto>> GetSites()
        {
            var result = SiteHelper.GetSites();
            return ApiResult<List<SiteDto>>.Ok(result);
        }

        public ApiResult<TileDto> GetTile(int tileId)
        {
            var result = TileHelper.GetTile(tileId);
            if (result == null)
            {
                return ApiResult<TileDto>.Fail($"Tile with id {tileId} not found.");
            }
            return ApiResult<TileDto>.Ok(result);
        }

        public ApiResult<List<TileDto>> GetTilesInRadius(int tileId, float radius)
        {
            var result = GlobalMapHelper.GetTilesInRadius(tileId, radius);
            return ApiResult<List<TileDto>>.Ok(result);
        }

        public ApiResult<CoordinatesDto> GetTileCoordinates(int tileId)
        {
            var result = GlobalMapHelper.GetTileCoordinates(tileId);
            if (result == null)
            {
                return ApiResult<CoordinatesDto>.Fail($"Tile with id {tileId} not found.");
            }
            return ApiResult<CoordinatesDto>.Ok(result);
        }

        public ApiResult<TileDetailsDto> GetTileDetails(int tileId)
        {
            var result = TileHelper.GetTileDetails(tileId);
            if (result == null)
            {
                return ApiResult<TileDetailsDto>.Fail($"Tile with id {tileId} not found.");
            }
            return ApiResult<TileDetailsDto>.Ok(result);
        }
    }
}
