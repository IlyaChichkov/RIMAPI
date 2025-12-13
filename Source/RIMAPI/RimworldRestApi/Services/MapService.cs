using System;
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using Verse;

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

        public ApiResult SetWeather(int mapId, string weatherDefName)
        {
            try
            {
                var map = MapHelper.GetMapByID(mapId);

                WeatherDef weatherDef = DefDatabase<WeatherDef>.GetNamed(weatherDefName, false);
                if (weatherDef == null)
                {
                    return ApiResult.Fail(
                        $"Could not find weather Def with name: {weatherDefName}"
                    );
                }

                map.weatherManager.TransitionTo(weatherDef);
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult<List<ThingDto>> GetThingsAtCell(int mapId, PositionDto position)
        {
            try
            {
                var map = MapHelper.GetMapByID(mapId);
                if (map == null)
                {
                    return ApiResult<List<ThingDto>>.Fail($"Map with ID {mapId} not found.");
                }

                if (position == null)
                {
                    return ApiResult<List<ThingDto>>.Fail("Position cannot be null.");
                }

                IntVec3 cellPosition = new IntVec3(position.X, position.Y, position.Z);

                List<Thing> things = cellPosition.GetThingList(map);
                List<ThingDto> thingDtos = new List<ThingDto>();

                foreach (Thing thing in things)
                {
                    thingDtos.Add(ResourcesHelper.ThingToDto(thing));
                }

                return ApiResult<List<ThingDto>>.Ok(thingDtos);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error getting things at cell: {ex}");
                return ApiResult<List<ThingDto>>.Fail(
                    $"Failed to get things at cell: {ex.Message}"
                );
            }
        }
    }
}
