

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimworldRestApi.Models;
using UnityEngine;
using Verse;

namespace RimworldRestApi.Helpers
{
    public class MapHelper
    {
        public Map FindMapByUniqueID(int uniqueID)
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

        public List<MapDto> GetMaps()
        {
            var maps = new List<MapDto>();

            try
            {
                foreach (var map in Current.Game.Maps)
                {
                    maps.Add(new MapDto
                    {
                        ID = map.uniqueID,
                        Index = map.Index,
                        Seed = map.ConstantRandSeed,
                        FactionID = map.ParentFaction.loadID.ToString(),
                        IsPlayerHome = map.IsPlayerHome,
                        IsPocketMap = map.IsPocketMap,
                        IsTempIncidentMap = map.IsTempIncidentMap,
                        Size = map.Size.ToString()
                    });
                }

                return maps;
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error - {ex.Message}");
                return maps;
            }
        }

        public MapCreaturesSummaryDto GetMapCreaturesSummary(int mapId)
        {
            try
            {
                Log.Message($"RIMAPI: Error - {mapId}");
                var map = FindMapByUniqueID(mapId);
                Log.Message($"RIMAPI: Error - {map == null}");
                return new MapCreaturesSummaryDto
                {
                    ColonistsCount = map.mapPawns.FreeColonistsSpawnedCount,
                    PrisonersCount = map.mapPawns.PrisonersOfColonyCount,
                    EnemiesCount = map.mapPawns.AllPawnsSpawned.Count(p =>
                                    p.RaceProps.Humanlike && p.HostileTo(Faction.OfPlayer)),
                    AnimalsCount = map.mapPawns.AllPawnsSpawned.Count(p => p.RaceProps.Animal),
                    InsectoidsCount = map.mapPawns.AllPawnsSpawned.Count(p => p != null &&
                                                                         p.Faction != null &&
                                                                         p.Faction.def == FactionDefOf.Insect),
                    MechanoidsCount = map.mapPawns.AllPawnsSpawned.Count(p => p != null &&
                                                                         p.RaceProps != null &&
                                                                         p.RaceProps.IsMechanoid),
                };
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error - {ex}");
                Log.Error($"RIMAPI: Error - {ex.Message}");
                return new MapCreaturesSummaryDto();
            }
        }

        public MapTimeDto GetDatetimeAt(int tileID)
        {
            MapTimeDto mapTimeDto = new MapTimeDto();
            try
            {
                if (Current.ProgramState != ProgramState.Playing || Find.WorldGrid == null)
                {
                    return mapTimeDto;
                }

                var vector = Find.WorldGrid.LongLatOf(GetMapTileId(Find.CurrentMap));
                mapTimeDto.Datetime = GenDate.DateFullStringWithHourAt(Find.TickManager.TicksAbs, vector);

                return mapTimeDto;
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error - {ex.Message}");
                return mapTimeDto;
            }
        }

        public MapPowerInfoDto GetMapPowerInfoInternal(int mapId)
        {
            MapPowerInfoDto powerInfo = new MapPowerInfoDto();

            try
            {
                Map map = FindMapByUniqueID(mapId);

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
                        powerInfo.CurrentlyStoredPower += Mathf.RoundToInt(powerBattery.StoredEnergy);
                        powerInfo.TotalPowerStorage += Mathf.RoundToInt(powerBattery.Props.storedEnergyMax);
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
                            powerInfo.TotalConsumption += Mathf.RoundToInt(comp.Props.PowerConsumption);
                        }
                        if (comp.PowerOn && comp.PowerOutput < 0f)
                        {
                            powerInfo.ConsumptionPowerOn += Mathf.RoundToInt(Mathf.Abs(comp.PowerOutput));
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
                Log.Error($"RIMAPI: Error - {ex.Message}");
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

        public List<AnimalDto> GetMapAnimals(int mapId)
        {
            List<AnimalDto> animals = new List<AnimalDto>();
            try
            {
                Map map = FindMapByUniqueID(mapId);
                if (map == null)
                {
                    return animals;
                }

                animals = map.mapPawns.AllPawns
                    .Where(p => p.RaceProps?.Animal == true)
                    .Select(p => new AnimalDto
                    {
                        Id = p.thingIDNumber,
                        Name = p.LabelShortCap,
                        Def = p.def?.defName,
                        Faction = p.Faction?.ToString(),
                        Position = new PositionDto { X = p.Position.x, Y = p.Position.z },
                        Trainer = p.relations?.DirectRelations
                            .Where(r => r.def == PawnRelationDefOf.Bond)
                            .Select(r => r.otherPawn?.thingIDNumber)
                            .FirstOrDefault(),
                        Pregnant = p.health?.hediffSet?.HasHediff(HediffDefOf.Pregnant) ?? false,
                    })
                    .ToList();

                return animals;
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error - {ex.Message}");
                return new List<AnimalDto>();
            }
        }

        public List<MapThingDto> GetMapThings(int mapId)
        {
            List<MapThingDto> things = new List<MapThingDto>();
            try
            {
                Map map = FindMapByUniqueID(mapId);
                if (map == null)
                {
                    return things;
                }

                things = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
                    .Select(p => new MapThingDto
                    {
                        Id = p.thingIDNumber,
                        Type = p.GetType().FullName,
                        Def = p.def?.defName,
                        Position = new PositionDto { X = p.Position.x, Y = p.Position.z },
                        IsForbidden = p.IsForbidden(Faction.OfPlayer),
                    })
                    .ToList();

                return things;
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error - {ex.Message}");
                return new List<MapThingDto>();
            }
        }

        public List<ZoneDto> GetMapZones(int mapId)
        {
            List<ZoneDto> zones = new List<ZoneDto>();
            try
            {
                Map map = FindMapByUniqueID(mapId);
                if (map == null)
                {
                    throw new Exception("Map with this id wasn't found");
                }

                foreach (Zone zone in map.zoneManager.AllZones)
                {
                    zones.Add(new ZoneDto
                    {
                        Id = zone.ID,
                        CellsCount = zone.CellCount,
                        Label = zone.label,
                        BaseLabel = zone.BaseLabel,
                        Type = zone.GetType().Name,
                    });
                }

                return zones;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<ZoneDto> GetMapAreas(int mapId)
        {
            List<ZoneDto> zones = new List<ZoneDto>();
            try
            {
                Map map = FindMapByUniqueID(mapId);
                if (map == null)
                {
                    throw new Exception("Map with this id wasn't found");
                }

                foreach (Area area in map.areaManager.AllAreas)
                {
                    zones.Add(new ZoneDto
                    {
                        Id = area.ID,
                        CellsCount = area.ActiveCells.Count(),
                        Label = area.Label,
                        BaseLabel = area.Label,
                        Type = area.GetType().Name,
                    });
                }

                return zones;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<BuildingDto> GetMapBuildings(int mapId)
        {
            List<BuildingDto> buildings = new List<BuildingDto>();
            Map map = FindMapByUniqueID(mapId);
            if (map == null)
            {
                throw new Exception("Map with this id wasn't found");
            }

            foreach (Building building in map.listerBuildings.allBuildingsColonist)
            {
                buildings.Add(new BuildingDto
                {
                    Id = building.thingIDNumber,
                    Def = building.def.defName,
                    Label = building.Label,
                    Position = new PositionDto
                    {
                        X = building.Position.x,
                        Y = building.Position.y,
                        Z = building.Position.z
                    },
                    Type = building.GetType().Name,
                });
            }

            return buildings;
        }
    }
}