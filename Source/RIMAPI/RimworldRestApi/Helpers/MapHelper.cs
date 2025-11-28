using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Helpers
{
    public static class MapHelper
    {
        public static Map GetMapByID(int uniqueID)
        {
            foreach (Map map in Find.Maps)
            {
                if (map.uniqueID == uniqueID)
                {
                    return map;
                }
            }
            return null;
        }

        public static List<MapDto> GetMaps()
        {
            var maps = new List<MapDto>();

            try
            {
                foreach (var map in Current.Game.Maps)
                {
                    maps.Add(
                        new MapDto
                        {
                            Id = map.uniqueID,
                            Index = map.Index,
                            Seed = map.ConstantRandSeed,
                            FactionId = map.ParentFaction.loadID.ToString(),
                            IsPlayerHome = map.IsPlayerHome,
                            IsPocketMap = map.IsPocketMap,
                            IsTempIncidentMap = map.IsTempIncidentMap,
                            Size = map.Size.ToString(),
                        }
                    );
                }

                return maps;
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error - {ex.Message}");
                return maps;
            }
        }

        public static MapCreaturesSummaryDto GetMapCreaturesSummary(int mapId)
        {
            try
            {
                var map = GetMapByID(mapId);
                return new MapCreaturesSummaryDto
                {
                    ColonistsCount = map.mapPawns.FreeColonistsSpawnedCount,
                    PrisonersCount = map.mapPawns.PrisonersOfColonyCount,
                    EnemiesCount = map.mapPawns.AllPawnsSpawned.Count(p =>
                        p.RaceProps.Humanlike && p.HostileTo(Faction.OfPlayer)
                    ),
                    AnimalsCount = map.mapPawns.AllPawnsSpawned.Count(p => p.RaceProps.Animal),
                    InsectoidsCount = map.mapPawns.AllPawnsSpawned.Count(p =>
                        p != null && p.Faction != null && p.Faction.def == FactionDefOf.Insect
                    ),
                    MechanoidsCount = map.mapPawns.AllPawnsSpawned.Count(p =>
                        p != null && p.RaceProps != null && p.RaceProps.IsMechanoid
                    ),
                };
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error - {ex}");
                Core.LogApi.Error($"Error - {ex.Message}");
                return new MapCreaturesSummaryDto();
            }
        }

        public static MapTimeDto GetDatetimeAt(int tileID)
        {
            MapTimeDto mapTimeDto = new MapTimeDto();
            try
            {
                if (Current.ProgramState != ProgramState.Playing || Find.WorldGrid == null)
                {
                    return mapTimeDto;
                }

                var vector = Find.WorldGrid.LongLatOf(GetMapTileId(Find.CurrentMap));
                mapTimeDto.Datetime = GenDate.DateFullStringWithHourAt(
                    Find.TickManager.TicksAbs,
                    vector
                );

                return mapTimeDto;
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error - {ex.Message}");
                return mapTimeDto;
            }
        }

        public static MapPowerInfoDto GetMapPowerInfoInternal(int mapId)
        {
            MapPowerInfoDto powerInfo = new MapPowerInfoDto();

            try
            {
                Map map = GetMapByID(mapId);

                foreach (Building building in map.listerBuildings.allBuildingsColonist)
                {
                    // Check if building is - Power Generator
                    CompPowerPlant powerPlant = building.TryGetComp<CompPowerPlant>();
                    if (powerPlant != null)
                    {
                        powerInfo.TotalPossiblePower += Mathf.RoundToInt(
                            Mathf.Abs(powerPlant.Props.PowerConsumption)
                        );
                        powerInfo.CurrentPower += Mathf.RoundToInt(powerPlant.PowerOutput);
                        powerInfo.ProducePowerBuildings.Add(building.thingIDNumber);
                        continue;
                    }

                    // Check if building is - Battery
                    CompPowerBattery powerBattery = building.TryGetComp<CompPowerBattery>();
                    if (powerBattery != null)
                    {
                        powerInfo.CurrentlyStoredPower += Mathf.RoundToInt(
                            powerBattery.StoredEnergy
                        );
                        powerInfo.TotalPowerStorage += Mathf.RoundToInt(
                            powerBattery.Props.storedEnergyMax
                        );
                        powerInfo.StorePowerBuildings.Add(building.thingIDNumber);
                    }
                }

                // Calculate power consumption
                foreach (PowerNet net in map.powerNetManager.AllNetsListForReading)
                {
                    foreach (CompPowerTrader comp in net.powerComps)
                    {
                        if (comp.Props.PowerConsumption > 0f)
                        {
                            powerInfo.TotalConsumption += Mathf.RoundToInt(
                                comp.Props.PowerConsumption
                            );
                        }
                        if (comp.PowerOn && comp.PowerOutput < 0f)
                        {
                            powerInfo.ConsumptionPowerOn += Mathf.RoundToInt(
                                Mathf.Abs(comp.PowerOutput)
                            );
                        }

                        Building building = comp.parent as Building;
                        if (building != null)
                        {
                            powerInfo.ConsumePowerBuildings.Add(building.thingIDNumber);
                        }
                    }
                }

                return powerInfo;
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error - {ex.Message}");
                return powerInfo;
            }
        }

        public static int GetMapTileId(Map map)
        {
#if RIMWORLD_1_5
            return map.Tile;
#elif RIMWORLD_1_6
            return map.Tile.tileId;
#endif
            throw new Exception("Failed to get GetMapTileId for this rimworld version.");
        }

        public static List<AnimalDto> GetMapAnimals(int mapId)
        {
            List<AnimalDto> animals = new List<AnimalDto>();
            try
            {
                Map map = GetMapByID(mapId);
                if (map == null)
                {
                    return animals;
                }

                animals = map
                    .mapPawns.AllPawns.Where(p => p.RaceProps?.Animal == true)
                    .Select(p => new AnimalDto
                    {
                        Id = p.thingIDNumber,
                        Name = p.LabelShortCap,
                        Def = p.def?.defName,
                        Faction = p.Faction?.ToString(),
                        Position = new PositionDto { X = p.Position.x, Y = p.Position.z },
                        Trainer = p
                            .relations?.DirectRelations.Where(r => r.def == PawnRelationDefOf.Bond)
                            .Select(r => r.otherPawn?.thingIDNumber)
                            .FirstOrDefault(),
                        Pregnant = p.health?.hediffSet?.HasHediff(HediffDefOf.Pregnant) ?? false,
                    })
                    .ToList();

                return animals;
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error - {ex.Message}");
                return new List<AnimalDto>();
            }
        }

        public static List<ThingDto> GetMapThings(int mapId)
        {
            List<ThingDto> things = new List<ThingDto>();
            try
            {
                Map map = GetMapByID(mapId);
                if (map == null)
                {
                    return things;
                }

                things = map
                    .listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                    .Select(p => ResourcesHelper.ThingToDto(p))
                    .ToList();

                return things;
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error - {ex.Message}");
                return new List<ThingDto>();
            }
        }

        public static List<ZoneDto> GetMapZones(int mapId)
        {
            List<ZoneDto> zones = new List<ZoneDto>();
            try
            {
                Map map = GetMapByID(mapId);
                if (map == null)
                {
                    throw new Exception("Map with this id wasn't found");
                }

                foreach (Zone zone in map.zoneManager.AllZones)
                {
                    zones.Add(
                        new ZoneDto
                        {
                            Id = zone.ID,
                            CellsCount = zone.CellCount,
                            Label = zone.label,
                            BaseLabel = zone.BaseLabel,
                            Type = zone.GetType().Name,
                        }
                    );
                }

                return zones;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<ZoneDto> GetMapAreas(int mapId)
        {
            List<ZoneDto> zones = new List<ZoneDto>();
            try
            {
                Map map = GetMapByID(mapId);
                if (map == null)
                {
                    throw new Exception("Map with this id wasn't found");
                }

                foreach (Area area in map.areaManager.AllAreas)
                {
                    zones.Add(
                        new ZoneDto
                        {
                            Id = area.ID,
                            CellsCount = area.ActiveCells.Count(),
                            Label = area.Label,
                            BaseLabel = area.Label,
                            Type = area.GetType().Name,
                        }
                    );
                }

                return zones;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<BuildingDto> GetMapBuildings(int mapId)
        {
            List<BuildingDto> buildings = new List<BuildingDto>();
            Map map = GetMapByID(mapId);
            if (map == null)
            {
                throw new Exception("Map with this id wasn't found");
            }

            foreach (Building building in map.listerBuildings.allBuildingsColonist)
            {
                buildings.Add(
                    new BuildingDto
                    {
                        Id = building.thingIDNumber,
                        Def = building.def.defName,
                        Label = building.Label,
                        Position = new PositionDto
                        {
                            X = building.Position.x,
                            Y = building.Position.y,
                            Z = building.Position.z,
                        },
                        Type = building.GetType().Name,
                    }
                );
            }

            return buildings;
        }

        public static MapRoomsDto GetRooms(Map map)
        {
            var mapRooms = new MapRoomsDto();
#if RIMWORLD_1_5
            List<Room> allRooms = map.regionGrid.allRooms;
            mapRooms = new MapRoomsDto
            {
                Rooms = allRooms
                    .Select(s => new RoomDto
                    {
                        Id = s.ID,
                        RoleLabel = s.GetRoomRoleLabel(),
                        Temperature = s.Temperature,
                        CellsCount = s.CellCount,
                        TouchesMapEdge = s.TouchesMapEdge,
                        IsPrisonCell = s.IsPrisonCell,
                        IsDoorway = s.IsDoorway,
                        ContainedBedsIds = s.ContainedBeds.Select(b => b.thingIDNumber).ToList(),
                        OpenRoofCount = s.OpenRoofCount,
                    })
                    .ToList(),
            };
#elif RIMWORLD_1_6
            var allRooms = map.regionGrid.AllRooms;
            mapRooms = new MapRoomsDto
            {
                Rooms = allRooms
                    .Select(s => new RoomDto
                    {
                        Id = s.ID,
                        RoleLabel = s.GetRoomRoleLabel(),
                        Temperature = s.Temperature,
                        CellsCount = s.CellCount,
                        TouchesMapEdge = s.TouchesMapEdge,
                        IsPrisonCell = s.IsPrisonCell,
                        IsDoorway = s.IsDoorway,
                        ContainedBedsIds = s.ContainedBeds.Select(b => b.thingIDNumber).ToList(),
                        OpenRoofCount = s.OpenRoofCount,
                    })
                    .ToList(),
            };
#endif
            return mapRooms;
        }
    }
}
