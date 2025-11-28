using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public class MapService : IMapService
    {
        public MapService() { }

        public ApiResult<MapFarmSummaryDto> GenerateFarmSummary(int mapId)
        {
            var map = MapHelper.GetMapByID(mapId);
            var result = FarmHelper.GenerateFarmSummary(map);
            return ApiResult<MapFarmSummaryDto>.Ok(result);
        }

        public ApiResult<GrowingZoneDto> GetGrowingZoneById(int mapId, int zoneId)
        {
            var map = MapHelper.GetMapByID(mapId);
            var result = FarmHelper.GetGrowingZoneById(map, zoneId);
            return ApiResult<GrowingZoneDto>.Ok(result);
        }

        public ApiResult<List<AnimalDto>> GetMapAnimals(int mapId)
        {
            var result = MapHelper.GetMapAnimals(mapId);
            return ApiResult<List<AnimalDto>>.Ok(result);
        }

        public ApiResult<List<BuildingDto>> GetMapBuildings(int mapId)
        {
            var result = MapHelper.GetMapBuildings(mapId);
            return ApiResult<List<BuildingDto>>.Ok(result);
        }

        public ApiResult<MapCreaturesSummaryDto> GetMapCreaturesSummary(int mapId)
        {
            var result = MapHelper.GetMapCreaturesSummary(mapId);
            return ApiResult<MapCreaturesSummaryDto>.Ok(result);
        }

        public ApiResult<MapPowerInfoDto> GetMapPowerInfo(int mapId)
        {
            var result = MapHelper.GetMapPowerInfoInternal(mapId);
            return ApiResult<MapPowerInfoDto>.Ok(result);
        }

        public ApiResult<MapRoomsDto> GetMapRooms(int mapId)
        {
            var map = MapHelper.GetMapByID(mapId);
            var result = MapHelper.GetRooms(map);
            return ApiResult<MapRoomsDto>.Ok(result);
        }

        public ApiResult<List<MapDto>> GetMaps()
        {
            var result = MapHelper.GetMaps();
            return ApiResult<List<MapDto>>.Ok(result);
        }

        public ApiResult<List<ThingDto>> GetMapThings(int mapId)
        {
            var result = MapHelper.GetMapThings(mapId);
            return ApiResult<List<ThingDto>>.Ok(result);
        }

        public ApiResult<MapZonesDto> GetMapZones(int mapId)
        {
            MapZonesDto mapZones = new MapZonesDto();
            mapZones.Zones = MapHelper.GetMapZones(mapId);
            mapZones.Areas = MapHelper.GetMapAreas(mapId);
            return ApiResult<MapZonesDto>.Ok(mapZones);
        }

        public ApiResult<MapWeatherDto> GetWeather(int mapId)
        {
            var map = MapHelper.GetMapByID(mapId);
            var result = new MapWeatherDto
            {
                Weather = map.weatherManager?.curWeather?.defName,
                Temperature = map.mapTemperature?.OutdoorTemp ?? 0f,
            };
            return ApiResult<MapWeatherDto>.Ok(result);
        }
    }
}
