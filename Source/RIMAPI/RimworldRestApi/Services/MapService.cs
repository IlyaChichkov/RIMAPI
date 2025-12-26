using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
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

        public ApiResult<MapTerrainDto> GetMapTerrain(int mapId)
        {
            var result = MapHelper.GetMapTerrain(mapId);
            return ApiResult<MapTerrainDto>.Ok(result);
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

        public ApiResult<List<ThingDto>> GetMapThingsInRadius(int mapId, int x, int z, int radius)
        {
            var result = MapHelper.GetMapThingsInRadius(mapId, x, z, radius);
            return ApiResult<List<ThingDto>>.Ok(result);
        }

        public ApiResult<List<ThingDto>> GetMapPlants(int mapId)
        {
            var result = MapHelper.GetMapPlants(mapId);
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

        public ApiResult<List<ThingDto>> GetThingsAtCell(ThingsAtCellRequestDto body)
        {
            try
            {
                if (body == null)
                {
                    return ApiResult<List<ThingDto>>.Fail("Request body is null");
                }

                var map = MapHelper.GetMapByID(body.MapId);
                if (map == null)
                {
                    return ApiResult<List<ThingDto>>.Fail($"Map with ID {body.MapId} not found.");
                }

                if (body.Position == null)
                {
                    return ApiResult<List<ThingDto>>.Fail("Position cannot be null.");
                }

                IntVec3 cellPosition = new IntVec3(body.Position.X, body.Position.Y, body.Position.Z);

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

        /// <summary>
        /// Destroys all corpses (human and animal) on the specified map.
        /// </summary>
        public ApiResult DestroyCorpses(int mapId)
        {
            try
            {
                var map = MapHelper.GetMapByID(mapId);
                if (map == null) return ApiResult.Fail($"Map with ID {mapId} not found.");

                // Fetch all corpses efficiently using the map's ThingLister
                // ToList() is required because we are modifying the collection while iterating
                List<Thing> corpses = map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).ToList();
                int count = corpses.Count;

                foreach (var corpse in corpses)
                {
                    if (!corpse.Destroyed)
                    {
                        corpse.Destroy();
                    }
                }

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error destroying corpses: {ex}");
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Destroys all items on the map that are marked as Forbidden (Red X).
        /// </summary>
        public ApiResult DestroyForbiddenItems(int mapId)
        {
            try
            {
                var map = MapHelper.GetMapByID(mapId);
                if (map == null) return ApiResult.Fail($"Map with ID {mapId} not found.");

                // "HaulableEver" covers almost all items (resources, weapons, apparel)
                var items = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
                var toDestroy = new List<Thing>();

                // Filter for forbidden items
                foreach (var item in items)
                {
                    if (item.IsForbidden(Faction.OfPlayer))
                    {
                        toDestroy.Add(item);
                    }
                }

                int count = toDestroy.Count;
                foreach (var item in toDestroy)
                {
                    if (!item.Destroyed)
                    {
                        item.Destroy();
                    }
                }

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error destroying forbidden items: {ex}");
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Destroys ALL things (Items, Buildings, Pawns, Plants, Filth) within a rectangular area.
        /// Does not destroy the Terrain (Floor).
        /// </summary>
        public ApiResult DestroyThingsInRect(DestroyRectRequestDto request)
        {
            try
            {
                if (request.PointA == null || request.PointB == null)
                    return ApiResult.Fail("PointA and PointB must be defined.");

                var map = MapHelper.GetMapByID(request.MapId);
                if (map == null) return ApiResult.Fail($"Map with ID {request.MapId} not found.");

                // Create the rectangle defined by the two points
                IntVec3 start = new IntVec3(request.PointA.X, 0, request.PointA.Z);
                IntVec3 end = new IntVec3(request.PointB.X, 0, request.PointB.Z);
                CellRect rect = CellRect.FromLimits(start, end);

                int count = 0;

                // Iterate through every cell in the rectangle
                foreach (IntVec3 cell in rect)
                {
                    if (!cell.InBounds(map)) continue;

                    // Get all things in this single cell
                    // CRITICAL: Create a copy (.ToList()) because destroying removes them from the list we are iterating
                    var thingsInCell = cell.GetThingList(map).ToList();

                    foreach (var thing in thingsInCell)
                    {
                        if (!thing.Destroyed)
                        {
                            thing.Destroy();
                            count++;
                        }
                    }
                }

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error destroying area: {ex}");
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult RepairThingsAtPositions(RepairPositionsRequestDto request)
        {
            try
            {
                var map = MapHelper.GetMapByID(request.MapId);
                if (map == null) return ApiResult.Fail($"Map {request.MapId} not found.");

                int count = 0;
                foreach (var posDto in request.Positions)
                {
                    IntVec3 cell = new IntVec3(posDto.X, posDto.Y, posDto.Z);
                    if (cell.InBounds(map))
                    {
                        count += RepairThingsInCell(cell, map);
                    }
                }
                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        // --- NEW: Repair in Area ---
        public ApiResult RepairThingsInRect(RepairRectRequestDto request)
        {
            try
            {
                var map = MapHelper.GetMapByID(request.MapId);
                if (map == null) return ApiResult.Fail($"Map {request.MapId} not found.");

                IntVec3 start = new IntVec3(request.PointA.X, 0, request.PointA.Z);
                IntVec3 end = new IntVec3(request.PointB.X, 0, request.PointB.Z);
                CellRect rect = CellRect.FromLimits(start, end);

                int count = 0;
                foreach (IntVec3 cell in rect)
                {
                    if (cell.InBounds(map))
                    {
                        count += RepairThingsInCell(cell, map);
                    }
                }
                return ApiResult.Ok();
            }
            catch (Exception ex) { return ApiResult.Fail(ex.Message); }
        }

        // --- Helper Logic ---
        private int RepairThingsInCell(IntVec3 cell, Map map)
        {
            int repaired = 0;
            // Get all things in cell (Walls, Tables, Turrets, etc)
            List<Thing> things = cell.GetThingList(map);

            foreach (var thing in things)
            {
                // Check if it's a building AND has health
                if (thing.def.useHitPoints && thing.HitPoints < thing.MaxHitPoints)
                {
                    thing.HitPoints = thing.MaxHitPoints;
                    repaired++;
                }
            }
            return repaired;
        }
    }
}
