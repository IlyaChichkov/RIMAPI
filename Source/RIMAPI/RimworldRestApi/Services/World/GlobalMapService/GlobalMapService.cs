using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld.Planet;
using Verse;

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

        public ApiResult<CaravanPathDto> GetCaravanPath(int tileId)
        {
            Caravan caravan = CaravanHelper.GetCaravanById(tileId);

            if (caravan == null)
            {
                return ApiResult<CaravanPathDto>.Fail($"Caravan with ID {tileId} not found.");
            }

            // 2. Prepare the basic result
            var result = new CaravanPathDto
            {
                Id = caravan.ID,
                Moving = caravan.pather.Moving,
                CurrentTile = caravan.Tile,
                NextTile = -1,
                DestinationTile = -1,
                Progress = 0f,
            };

            // 3. Calculate pathing data if moving
            if (caravan.pather.Moving)
            {
                result.NextTile = caravan.pather.nextTile;
#if RIMWORLD_1_5
                result.DestinationTile = caravan.pather.Destination;
#elif RIMWORLD_1_6
                result.DestinationTile = caravan.pather.Destination.tileId;
#endif

                // Calculate progress (0.0 to 1.0)
                // Guard against division by zero if costTotal is somehow 0
                float costTotal = caravan.pather.nextTileCostTotal;
                float costLeft = caravan.pather.nextTileCostLeft;

                float progress = (costTotal > 0) ? (1f - (costLeft / costTotal)) : 0f;
                result.Progress = progress;

                result.Path = CaravanHelper.GetFullCaravanPath(caravan);
            }

            return ApiResult<CaravanPathDto>.Ok(result);
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
