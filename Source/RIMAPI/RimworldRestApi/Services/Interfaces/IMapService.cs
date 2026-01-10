using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Models.Map;

namespace RIMAPI.Services
{
    public interface IMapService
    {
        ApiResult<List<MapDto>> GetMaps();
        ApiResult<MapPowerInfoDto> GetMapPowerInfo(int mapId);
        ApiResult<MapWeatherDto> GetWeather(int mapId);
        ApiResult<List<AnimalDto>> GetMapAnimals(int mapId);
        ApiResult<List<ThingDto>> GetMapThings(int mapId);
        ApiResult<List<ThingDto>> GetMapPlants(int mapId);
        ApiResult<MapCreaturesSummaryDto> GetMapCreaturesSummary(int mapId);
        ApiResult<MapFarmSummaryDto> GenerateFarmSummary(int mapId);
        ApiResult<GrowingZoneDto> GetGrowingZoneById(int mapId, int zoneId);
        ApiResult<MapZonesDto> GetMapZones(int mapId);
        ApiResult<MapRoomsDto> GetMapRooms(int mapId);
        ApiResult<List<BuildingDto>> GetMapBuildings(int mapId);
        ApiResult<MapTerrainDto> GetMapTerrain(int mapId);
        ApiResult<List<ThingDto>> GetMapThingsInRadius(int mapId, int x, int z, int radius);
        ApiResult SetWeather(int mapId, string defName);
        ApiResult<List<ThingDto>> GetThingsAtCell(ThingsAtCellRequestDto body);
        ApiResult DestroyCorpses(int mapId);
        ApiResult DestroyForbiddenItems(int mapId);
        ApiResult DestroyThingsInRect(DestroyRectRequestDto request);
        ApiResult RepairThingsAtPositions(RepairPositionsRequestDto request);
        ApiResult RepairThingsInRect(RepairRectRequestDto request);
        ApiResult SpawnDropPod(SpawnDropPodRequestDto request);
        ApiResult<FogGridDto> GetFogGrid(int mapId);
        ApiResult<OreDataDto> GetMapOre(int mapId);
    }
}
