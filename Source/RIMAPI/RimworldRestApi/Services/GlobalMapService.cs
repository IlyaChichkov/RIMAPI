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
    }
}
