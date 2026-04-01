using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Models.Map;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Helpers
{
    public static class MapHelper
    {
        public static Thing GetThingOnMapById(int mapId, int id)
        {
            Map map = GetMapByID(mapId);
            return map.listerThings.AllThings.Where(s => s.thingIDNumber == id).FirstOrDefault();
        }

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

        public static List<ThingDto> GetMapPlants(int mapId)
        {
            List<ThingDto> plants = new List<ThingDto>();
            try
            {
                Map map = GetMapByID(mapId);
                if (map == null)
                {
                    return plants;
                }

                // Get all plants (trees, bushes, crops, etc.)
                plants = map
                    .listerThings.ThingsInGroup(ThingRequestGroup.Plant)
                    .Select(p => ResourcesHelper.ThingToDto(p))
                    .ToList();

                return plants;
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
                        Rotation = building.Rotation.AsInt,
                        Size = new PositionDto
                        {
                            X = building.def.size.x,
                            Y = 0,
                            Z = building.def.size.z
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

        public static MapTerrainDto GetMapTerrain(int mapId)
        {
            var map = GetMapByID(mapId);
            if (map == null) return new MapTerrainDto();

            var terrainGrid = map.terrainGrid;
            var size = map.Size;
            int width = size.x;
            int height = size.z;

            // 1. Build Palette and Raw Index Grid
            var palette = new List<string>();
            var paletteLookup = new Dictionary<TerrainDef, int>();
            var rawIndices = new int[width * height];

            int cellIndex = 0;
            // Iterate Z then X (Standard loop order)
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    TerrainDef def = terrainGrid.TerrainAt(new IntVec3(x, 0, z));

                    if (!paletteLookup.TryGetValue(def, out int pIndex))
                    {
                        pIndex = palette.Count;
                        palette.Add(def.defName);
                        paletteLookup[def] = pIndex;
                    }

                    rawIndices[cellIndex++] = pIndex;
                }
            }

            // 2. Run-Length Encoding (RLE)
            var compressedGrid = new List<int>();
            if (rawIndices.Length > 0)
            {
                int currentVal = rawIndices[0];
                int count = 1;

                for (int i = 1; i < rawIndices.Length; i++)
                {
                    if (rawIndices[i] == currentVal)
                    {
                        count++;
                    }
                    else
                    {
                        compressedGrid.Add(count);
                        compressedGrid.Add(currentVal);
                        currentVal = rawIndices[i];
                        count = 1;
                    }
                }
                // Add final run
                compressedGrid.Add(count);
                compressedGrid.Add(currentVal);
            }

            // 3. Build Floor Palette and Grid (for constructed floors)
            var floorPalette = new List<string>();
            var floorPaletteLookup = new Dictionary<string, int>();
            var rawFloorIndices = new int[width * height];

            cellIndex = 0;
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = new IntVec3(x, 0, z);
                    var building = map.edificeGrid.InnerArray[cellIndex];

                    // Check if this is a floor (constructed floor blueprint)
                    // Floors in RimWorld are Buildings that have a graphic but no altitude (they're on the ground)
                    string floorDefName = null;
                    if (building != null && building.def.building != null)
                    {
                        // Floors typically have graphicData and no altitudeLayer set
                        // Or check if the defName contains "Floor"
                        if (building.def.graphicData != null && building.def.altitudeLayer == Verse.AltitudeLayer.Floor)
                        {
                            floorDefName = building.def.defName;
                        }
                    }

                    int fIndex = 0; // 0 = no floor (null)
                    if (floorDefName != null)
                    {
                        if (!floorPaletteLookup.TryGetValue(floorDefName, out fIndex))
                        {
                            fIndex = floorPalette.Count + 1; // +1 because 0 is reserved for null
                            floorPalette.Add(floorDefName);
                            floorPaletteLookup[floorDefName] = fIndex;
                        }
                    }

                    rawFloorIndices[cellIndex++] = fIndex;
                }
            }

            // 4. Run-Length Encoding for Floors
            var compressedFloorGrid = new List<int>();
            if (rawFloorIndices.Length > 0)
            {
                int currentVal = rawFloorIndices[0];
                int count = 1;

                for (int i = 1; i < rawFloorIndices.Length; i++)
                {
                    if (rawFloorIndices[i] == currentVal)
                    {
                        count++;
                    }
                    else
                    {
                        compressedFloorGrid.Add(count);
                        compressedFloorGrid.Add(currentVal);
                        currentVal = rawFloorIndices[i];
                        count = 1;
                    }
                }
                // Add final run
                compressedFloorGrid.Add(count);
                compressedFloorGrid.Add(currentVal);
            }

            return new MapTerrainDto
            {
                Width = width,
                Height = height,
                Palette = palette,
                Grid = compressedGrid,
                FloorPalette = floorPalette,
                FloorGrid = compressedFloorGrid
            };
        }

        public static List<ThingDto> GetMapThingsInRadius(int mapId, int centerX, int centerZ, int radius)
        {
            var results = new List<ThingDto>();
            Map map = GetMapByID(mapId);
            if (map == null) return results;

            IntVec3 center = new IntVec3(centerX, 0, centerZ);
            var processedIds = new HashSet<int>();

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!cell.InBounds(map)) continue;
                List<Thing> thingsAtCell = map.thingGrid.ThingsListAt(cell);

                for (int i = 0; i < thingsAtCell.Count; i++)
                {
                    Thing t = thingsAtCell[i];
                    if (processedIds.Contains(t.thingIDNumber)) continue;

                    // Filter: Items, Buildings, Plants
                    if (t.def.category == ThingCategory.Item ||
                        t.def.category == ThingCategory.Building ||
                        t.def.category == ThingCategory.Plant)
                    {
                        // Skip invisible things
                        if (t.def.drawerType == DrawerType.None) continue;

                        // Convert to DTO
                        // Use existing helper but ensure we capture building-specifics if needed
                        var dto = ResourcesHelper.ThingToDto(t);

                        // Correction for Buildings: Size/Rotation logic in ThingToDto is generic
                        // Ensure it matches what we need

                        results.Add(dto);
                        processedIds.Add(t.thingIDNumber);
                    }
                }
            }
            return results;
        }

        public static OreDataDto GetOreData(int mapId)
        {
            Map map = MapHelper.GetMapByID(mapId);
            if (map == null) return null;

            int width = map.Size.x;

            var oreData = new OreDataDto
            {
                MapWidth = width,
                Ores = new Dictionary<string, OreGroupDto>()
            };

            // Use a single loop through AllThings to populate the groups
            // This is faster than LINQ GroupBy for large datasets in Unity/RimWorld
            foreach (var thing in map.listerThings.AllThings)
            {
                if (thing.def.mineable)
                {
                    string defName = thing.def.defName;

                    // Get or Create the Group for this ore type
                    if (!oreData.Ores.TryGetValue(defName, out OreGroupDto group))
                    {
                        group = new OreGroupDto
                        {
                            MaxHp = thing.MaxHitPoints,
                            Cells = new List<int>(),
                            Hp = new List<int>()
                        };
                        oreData.Ores[defName] = group;
                    }

                    // Flatten Position (X, Z) into a single integer Index
                    // Formula: index = (z * width) + x
                    // We ignore Y because ores are always on the surface layer in RimWorld
                    int index = (thing.Position.z * width) + thing.Position.x;

                    group.Cells.Add(index);
                    group.Hp.Add(thing.HitPoints);
                }
            }

            return oreData;
        }

        public static StockpileResponseDto CreateStockpile(CreateStockpileRequestDto request)
        {
            try
            {
                var map = GetMapByID(request.MapId);
                if (map == null)
                {
                    return new StockpileResponseDto
                    {
                        Success = false,
                        Message = $"Map with ID {request.MapId} not found."
                    };
                }

                // Validate positions
                if (request.PointA == null || request.PointB == null)
                {
                    return new StockpileResponseDto
                    {
                        Success = false,
                        Message = "PointA and PointB cannot be null."
                    };
                }

                // Convert positions to IntVec3
                IntVec3 pointA = new IntVec3(request.PointA.X, request.PointA.Y, request.PointA.Z);
                IntVec3 pointB = new IntVec3(request.PointB.X, request.PointB.Y, request.PointB.Z);

                // Normalize rectangle (ensure min/max are in correct order)
                int minX = Mathf.Min(pointA.x, pointB.x);
                int maxX = Mathf.Max(pointA.x, pointB.x);
                int minZ = Mathf.Min(pointA.z, pointB.z);
                int maxZ = Mathf.Max(pointA.z, pointB.z);

                // Validate that rectangle has at least one cell
                if (minX > maxX || minZ > maxZ)
                {
                    return new StockpileResponseDto
                    {
                        Success = false,
                        Message = "Invalid rectangle: PointA and PointB form an invalid area."
                    };
                }

                // Create CellRect from normalized coordinates
                IntVec3 cellRectA = new IntVec3(minX, 0, minZ);
                IntVec3 cellRectB = new IntVec3(maxX, 0, maxZ);
                CellRect rect = CellRect.FromLimits(cellRectA, cellRectB);

                // Validate cells are in bounds
                if (!rect.All(cell => cell.InBounds(map)))
                {
                    return new StockpileResponseDto
                    {
                        Success = false,
                        Message = "Some cells in the specified rectangle are out of map bounds."
                    };
                }

                if (rect.Area == 0)
                {
                    return new StockpileResponseDto
                    {
                        Success = false,
                        Message = "No valid cells found in the specified rectangle."
                    };
                }

                // Create new stockpile zone
                Zone_Stockpile stockpile = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);

                // Set name BEFORE registering
                if (string.IsNullOrEmpty(request.Name))
                {
                    // Auto-generate name: "Stockpile #"
                    int stockpileCount = map.zoneManager.AllZones.OfType<Zone_Stockpile>().Count() + 1;
                    stockpile.label = $"Stockpile {stockpileCount}";
                }
                else
                {
                    stockpile.label = request.Name;
                }

                // Set priority BEFORE registering
                int priorityValue = request.Priority ?? 0; // Default to High (0)
                priorityValue = Mathf.Clamp(priorityValue, -1, 2);
                stockpile.settings.Priority = (StoragePriority)priorityValue;

                // Configure storage settings (filtration)
                bool hasItemDefs = request.AllowedItemDefs != null && request.AllowedItemDefs.Count > 0;
                bool hasCategories = request.AllowedItemCategories != null && request.AllowedItemCategories.Count > 0;

                if (hasItemDefs || hasCategories)
                {
                    // Disallow all first
                    foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
                    {
                        stockpile.settings.filter.SetAllow(thingDef, false);
                    }

                    // Allow specified item defs
                    if (hasItemDefs)
                    {
                        foreach (var defName in request.AllowedItemDefs)
                        {
                            ThingDef thingDef = DefDatabase<ThingDef>.GetNamed(defName, false);
                            if (thingDef != null)
                            {
                                stockpile.settings.filter.SetAllow(thingDef, true);
                            }
                        }
                    }

                    // Allow items from specified categories
                    if (hasCategories)
                    {
                        foreach (var categoryName in request.AllowedItemCategories)
                        {
                            ThingCategoryDef categoryDef = DefDatabase<ThingCategoryDef>.GetNamed(categoryName, false);
                            if (categoryDef != null)
                            {
                                foreach (ThingDef thingDef in categoryDef.DescendantThingDefs)
                                {
                                    stockpile.settings.filter.SetAllow(thingDef, true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Default allowance: allow all items that storage zones normally allow
                    foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
                    {
                        if (thingDef.category == ThingCategory.Item && 
                            !thingDef.IsCorpse && 
                            thingDef.alwaysHaulable)
                        {
                            stockpile.settings.filter.SetAllow(thingDef, true);
                        }
                    }
                }

                // Set hit points filter
                float minHpPercent = request.MinHitPointsPercent ?? 0.0f;
                float maxHpPercent = request.MaxHitPointsPercent ?? 1.0f;
                minHpPercent = Mathf.Clamp01(minHpPercent);
                maxHpPercent = Mathf.Clamp01(maxHpPercent);
                
                if (minHpPercent > 0.0f || maxHpPercent < 1.0f)
                {
                    stockpile.settings.filter.AllowedHitPointsPercents = new Verse.FloatRange(minHpPercent, maxHpPercent);
                    stockpile.settings.filter.allowedHitPointsConfigurable = true;
                }

                // Set quality filter
                if (!string.IsNullOrEmpty(request.MinQuality) || !string.IsNullOrEmpty(request.MaxQuality))
                {
                    QualityCategory minQuality = QualityCategory.Awful;
                    QualityCategory maxQuality = QualityCategory.Legendary;

                    if (!string.IsNullOrEmpty(request.MinQuality))
                    {
                        if (System.Enum.TryParse<QualityCategory>(request.MinQuality, out var parsedMin))
                        {
                            minQuality = parsedMin;
                        }
                    }

                    if (!string.IsNullOrEmpty(request.MaxQuality))
                    {
                        if (System.Enum.TryParse<QualityCategory>(request.MaxQuality, out var parsedMax))
                        {
                            maxQuality = parsedMax;
                        }
                    }

                    stockpile.settings.filter.AllowedQualityLevels = new QualityRange(minQuality, maxQuality);
                    stockpile.settings.filter.allowedQualitiesConfigurable = true;
                }

                // IMPORTANT: Register the zone BEFORE adding cells to avoid conflicts
                map.zoneManager.RegisterZone(stockpile);

                // NOW add all cells to the registered zone
                foreach (IntVec3 cell in rect)
                {
                    stockpile.AddCell(cell);
                }

                int cellCount = rect.Area;
                LogApi.Info($"Created stockpile '{stockpile.label}' with {cellCount} cells at priority {stockpile.settings.Priority}");

                return new StockpileResponseDto
                {
                    Success = true,
                    ZoneId = stockpile.ID,
                    Name = stockpile.label,
                    CellsCount = cellCount,
                    Priority = (int)stockpile.settings.Priority,
                    Message = $"Stockpile '{stockpile.label}' created successfully with {cellCount} cells."
                };
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error creating stockpile: {ex}");
                return new StockpileResponseDto
                {
                    Success = false,
                    Message = $"Failed to create stockpile: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Helper method to find a stockpile zone by ID across all maps
        /// </summary>
        private static (Zone_Stockpile stockpile, Map map) FindStockpileById(int zoneId)
        {
            foreach (Map map in Find.Maps)
            {
                var zone = map.zoneManager.AllZones.OfType<Zone_Stockpile>()
                    .FirstOrDefault(z => z.ID == zoneId);
                
                if (zone != null)
                {
                    return (zone, map);
                }
            }
            return (null, null);
        }

        public static ApiResult DeleteStockpile(int zoneId)
        {
            try
            {
                var (stockpile, stockpileMap) = FindStockpileById(zoneId);

                if (stockpile == null)
                {
                    return ApiResult.Fail($"Stockpile with ID {zoneId} not found.");
                }

                string stockpileName = stockpile.label;
                int cellsCount = stockpile.CellCount;
                Map mapToCleanup = stockpileMap; // Capture for lambda
                Zone_Stockpile zoneToDelete = stockpile; // Capture for lambda

                // Execute on main thread to ensure proper cleanup
                LongEventHandler.ExecuteWhenFinished(() =>
                {
                    try
                    {
                        if (mapToCleanup != null && zoneToDelete != null)
                        {
                            // Clear all cells from the zone using Cells list directly
                            var cellsList = zoneToDelete.Cells;
                            if (cellsList != null && cellsList.Count > 0)
                            {
                                // Copy cells to avoid collection modification during iteration
                                var cellsToRemove = cellsList.ToList();
                                foreach (var cell in cellsToRemove)
                                {
                                    zoneToDelete.RemoveCell(cell);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("haul destination"))
                        {
                            LogApi.Warning($"Stockpile deletion warning: {ex.Message}");
                        }
                    }
                });

                LogApi.Info($"Deleted stockpile '{stockpileName}' (ID: {zoneId}) with {cellsCount} cells");
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error deleting stockpile: {ex}");
                return ApiResult.Fail($"Failed to delete stockpile: {ex.Message}");
            }
        }

        public static ApiResult<StockpileResponseDto> UpdateStockpile(UpdateStockpileRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return ApiResult<StockpileResponseDto>.Fail("Request cannot be null.");
                }

                var (stockpile, stockpileMap) = FindStockpileById(request.ZoneId);

                if (stockpile == null)
                {
                    return ApiResult<StockpileResponseDto>.Fail($"Stockpile with ID {request.ZoneId} not found.");
                }

                bool filterWasModified = false;

                if (!string.IsNullOrEmpty(request.Name))
                {
                    stockpile.label = request.Name;
                }

                if (request.Priority.HasValue)
                {
                    int priorityValue = Mathf.Clamp(request.Priority.Value, 0, 5);
                    stockpile.settings.Priority = (StoragePriority)priorityValue;
                }

                if (request.MinHitPointsPercent.HasValue || request.MaxHitPointsPercent.HasValue)
                {
                    float minHp = request.MinHitPointsPercent ?? stockpile.settings.filter.AllowedHitPointsPercents.min;
                    float maxHp = request.MaxHitPointsPercent ?? stockpile.settings.filter.AllowedHitPointsPercents.max;
                    
                    minHp = Mathf.Clamp01(minHp);
                    maxHp = Mathf.Clamp01(maxHp);
                    
                    stockpile.settings.filter.AllowedHitPointsPercents = new Verse.FloatRange(minHp, maxHp);
                    stockpile.settings.filter.allowedHitPointsConfigurable = true;
                    filterWasModified = true;
                }

                if (!string.IsNullOrEmpty(request.MinQuality) || !string.IsNullOrEmpty(request.MaxQuality))
                {
                    QualityCategory minQuality = stockpile.settings.filter.AllowedQualityLevels.min;
                    QualityCategory maxQuality = stockpile.settings.filter.AllowedQualityLevels.max;

                    if (!string.IsNullOrEmpty(request.MinQuality))
                    {
                        if (System.Enum.TryParse<QualityCategory>(request.MinQuality, out var parsedMin))
                        {
                            minQuality = parsedMin;
                        }
                    }

                    if (!string.IsNullOrEmpty(request.MaxQuality))
                    {
                        if (System.Enum.TryParse<QualityCategory>(request.MaxQuality, out var parsedMax))
                        {
                            maxQuality = parsedMax;
                        }
                    }

                    stockpile.settings.filter.AllowedQualityLevels = new QualityRange(minQuality, maxQuality);
                    stockpile.settings.filter.allowedQualitiesConfigurable = true;
                    filterWasModified = true;
                }

                if (request.RemoveItemDefs != null && request.RemoveItemDefs.Count > 0)
                {
                    foreach (var defName in request.RemoveItemDefs)
                    {
                        ThingDef thingDef = DefDatabase<ThingDef>.GetNamed(defName, false);
                        if (thingDef != null)
                        {
                            stockpile.settings.filter.SetAllow(thingDef, false);
                            filterWasModified = true;
                        }
                    }
                }

                if (request.RemoveItemCategories != null && request.RemoveItemCategories.Count > 0)
                {
                    foreach (var categoryName in request.RemoveItemCategories)
                    {
                        ThingCategoryDef categoryDef = DefDatabase<ThingCategoryDef>.GetNamed(categoryName, false);
                        if (categoryDef != null)
                        {
                            foreach (ThingDef thingDef in categoryDef.DescendantThingDefs)
                            {
                                stockpile.settings.filter.SetAllow(thingDef, false);
                                filterWasModified = true;
                            }
                        }
                    }
                }

                if (request.AddItemDefs != null && request.AddItemDefs.Count > 0)
                {
                    foreach (var defName in request.AddItemDefs)
                    {
                        ThingDef thingDef = DefDatabase<ThingDef>.GetNamed(defName, false);
                        if (thingDef != null)
                        {
                            stockpile.settings.filter.SetAllow(thingDef, true);
                            filterWasModified = true;
                        }
                    }
                }

                if (request.AddItemCategories != null && request.AddItemCategories.Count > 0)
                {
                    foreach (var categoryName in request.AddItemCategories)
                    {
                        ThingCategoryDef categoryDef = DefDatabase<ThingCategoryDef>.GetNamed(categoryName, false);
                        if (categoryDef != null)
                        {
                            foreach (ThingDef thingDef in categoryDef.DescendantThingDefs)
                            {
                                stockpile.settings.filter.SetAllow(thingDef, true);
                                filterWasModified = true;
                            }
                        }
                    }
                }

                // If filter was modified, resolve references to apply changes
                if (filterWasModified)
                {
                    try
                    {
                        stockpile.settings.filter.ResolveReferences();
                        LogApi.Info($"Applied filter changes to stockpile '{stockpile.label}'");
                    }
                    catch (Exception ex)
                    {
                        LogApi.Error($"Failed to apply filter changes: {ex.Message}");
                    }
                }

                LogApi.Info($"Updated stockpile '{stockpile.label}' (ID: {request.ZoneId})");

                return ApiResult<StockpileResponseDto>.Ok(new StockpileResponseDto
                {
                    Success = true,
                    ZoneId = stockpile.ID,
                    Name = stockpile.label,
                    CellsCount = stockpile.CellCount,
                    Priority = (int)stockpile.settings.Priority,
                    Message = $"Stockpile '{stockpile.label}' updated successfully."
                });
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error updating stockpile: {ex}");
                return ApiResult<StockpileResponseDto>.Fail($"Failed to update stockpile: {ex.Message}");
            }
        }
    }
}
