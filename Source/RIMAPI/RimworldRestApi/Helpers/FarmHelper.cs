using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimworldRestApi.Models;
using UnityEngine;
using Verse;


namespace RimworldRestApi.Helpers
{
    public class FarmHelper
    {
        public MapFarmSummaryDto GenerateFarmSummary(Map map)
        {
            var summary = new MapFarmSummaryDto();
            var growingZones = map.zoneManager.AllZones.OfType<Zone_Growing>().ToList();

            summary.TotalGrowingZones = growingZones.Count;

            var cropTypes = new Dictionary<string, CropTypeDto>();
            var allPlants = new List<Plant>();
            float totalDaysUntilHarvest = 0f;

            foreach (Zone_Growing zone in growingZones)
            {
                var zonePlants = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
                    .OfType<Plant>()
                    .Where(p => zone.ContainsCell(p.Position))
                    .ToList();

                allPlants.AddRange(zonePlants);
                var plantDefToGrowName = zone.PlantDefToGrow.defName;
                if (!cropTypes.ContainsKey(plantDefToGrowName))
                {
                    cropTypes[plantDefToGrowName] = new CropTypeDto
                    {
                        PlantDefName = plantDefToGrowName,
                        PlantLabel = zone.PlantDefToGrow.label,
                        PlantCategory = GetPlantCategory(zone.PlantDefToGrow),
                        TotalPlants = 0,
                        ExpectedYield = 0,
                        InfectedCount = 0,
                        HarvestablePlants = 0,
                        GrowthProgressAverage = 0,
                        DaysUntilHarvest = 0,
                        IsFullyGrown = false,
                        IsHarvestable = false,
                        ZoneId = zone.ID,
                    };
                }

                foreach (Plant plant in zonePlants)
                {
                    var defName = plant.def.defName;

                    if (!cropTypes.ContainsKey(defName))
                    {
                        continue;
                    }

                    var crop = cropTypes[defName];
                    crop.TotalPlants++;
                    crop.GrowthProgressAverage += plant.Growth;

                    if (plant.IsCrop && plant.HarvestableNow)
                    {
                        crop.HarvestablePlants++;
                        crop.ExpectedYield += plant.YieldNow();
                        crop.IsHarvestable = true;
                    }

                    if (plant.Blighted)
                    {
                        crop.InfectedCount++;
                        summary.TotalInfectedPlants++;
                    }

                    if (plant.Growth >= 1f)
                    {
                        crop.IsFullyGrown = true;
                    }

                    totalDaysUntilHarvest += CalculateDaysUntilHarvest(plant);
                }
            }

            // Calculate averages and finalize crop types
            foreach (var crop in cropTypes.Values)
            {
                if (crop.TotalPlants > 0)
                {
                    crop.DaysUntilHarvest = totalDaysUntilHarvest / crop.TotalPlants;
                }
                summary.TotalPlants += crop.TotalPlants;
                summary.TotalExpectedYield += crop.ExpectedYield;
                summary.GrowthProgressAverage += crop.GrowthProgressAverage;
            }

            if (cropTypes.Count > 0)
            {
                summary.GrowthProgressAverage /= cropTypes.Count;
            }

            summary.CropTypes = cropTypes.Values.OrderByDescending(c => c.TotalPlants).ToList();
            return summary;
        }

        public GrowingZoneDto GetGrowingZoneById(Map map, int zoneId)
        {
            var zone = map.zoneManager.AllZones.OfType<Zone_Growing>()
                .FirstOrDefault(z => z.ID == zoneId);

            if (zone == null) return null;

            var zonePlants = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
                .OfType<Plant>()
                .Where(p => zone.ContainsCell(p.Position))
                .ToList();

            var zoneDto = new GrowingZoneDto
            {
                ZoneId = zone.ID,
                CellsCount = zone.CellCount,
                ZoneLabel = zone.label ?? "Unnamed Zone",
                PlantDefName = zone.GetPlantDefToGrow()?.defName ?? "None",
                PlantCount = zonePlants.Count,
                ExpectedYield = 0,
                InfectedCount = 0,
                GrowthProgress = 0,
                HasDying = false,
                HasDyingFromPollution = false,
                HasDyingFromNoPollution = false,
                IsSowing = zone.allowSow &&
                           PlantUtility.GrowthSeasonNow(zone.Cells.FirstOrDefault(), map, forSowing: true) &&
                           zone.Cells.Any(c => c.GetPlant(map) == null),
                SoilType = GetSoilType(zone.Cells.FirstOrDefault(), map),
                Fertility = GetZoneFertility(zone, map)
            };

            if (zonePlants.Count > 0)
            {
                zoneDto.GrowthProgress = zonePlants.Average(p => p.Growth);
                zoneDto.DefExpectedYield = Mathf.RoundToInt(zonePlants
                    .Sum(p => p.def.plant.harvestYield));
                zoneDto.ExpectedYield = Mathf.RoundToInt(zonePlants
                    .Where(p => p.HarvestableNow)
                    .Sum(p => p.YieldNow()));
                zoneDto.InfectedCount = zonePlants.Count(p => p.Blighted);
                zoneDto.HasDying = zonePlants.Any(p => p.Dying);
                zoneDto.HasDyingFromPollution = zonePlants.Any(p => p.DyingFromPollution);
                zoneDto.HasDyingFromNoPollution = zonePlants.Any(p => p.DyingFromNoPollution);
            }

            return zoneDto;
        }

        // Helper methods
        private string GetPlantCategory(ThingDef plantDef)
        {
            if (plantDef == null) return "Unknown";
            if (plantDef.plant == null) return "Unknown";

            if (plantDef.plant.IsTree) return "Tree";
            if (plantDef.plant.harvestedThingDef?.IsDrug ?? false) return "Drug";
            if (plantDef.plant.harvestedThingDef?.IsMedicine ?? false) return "Medicine";
            if (plantDef.plant.Sowable) return "Crop";

            return "Other";
        }

        private float CalculateDaysUntilHarvest(Plant plant)
        {
            if (plant.Growth >= plant.def.plant.harvestMinGrowth)
            {
                return 0f; // Already harvestable  
            }

            float remainingGrowth = plant.def.plant.harvestMinGrowth - plant.Growth;
            int ticksUntilHarvest = (int)(remainingGrowth / plant.def.plant.growDays);
            return ticksUntilHarvest; // Convert ticks to days  
        }

        private bool IsPlantResting(Plant plant)
        {
            float dayPercent = GenLocalDate.DayPercent(plant);
            return dayPercent < 0.25f || dayPercent > 0.8f;
        }

        private string GetSoilType(IntVec3 cell, Map map)
        {
            if (!cell.InBounds(map)) return "Unknown";
            var terrain = map.terrainGrid.TerrainAt(cell);
            return terrain?.defName ?? "Unknown";
        }

        private float GetZoneFertility(Zone_Growing zone, Map map)
        {
            if (zone.Cells.Count == 0) return 0f;

            var fertilitySum = 0f;
            var sampleCells = zone.Cells.Take(10); // Sample first 10 cells for performance

            foreach (var cell in sampleCells)
            {
                if (cell.InBounds(map))
                {
                    fertilitySum += map.fertilityGrid.FertilityAt(cell);
                }
            }

            return sampleCells.Any() ? fertilitySum / sampleCells.Count() : 0f;
        }
    }
}