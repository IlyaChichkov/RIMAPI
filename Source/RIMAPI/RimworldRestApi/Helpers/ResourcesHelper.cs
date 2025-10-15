using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimworldRestApi.Models;
using UnityEngine;
using Verse;


namespace RimworldRestApi.Helpers
{
    public class ResourcesHelper
    {
        public ResourcesSummaryDto GenerateResourcesSummary(Map map)
        {
            try
            {
                var allItems = map.listerThings.AllThings
                    .Where(t => t.def.EverStorable(false))
                    .ToList();

                var summary = new ResourcesSummaryDto
                {
                    TotalItems = allItems.Count,
                    TotalMarketValue = allItems.Sum(t => t.MarketValue * t.stackCount),
                    LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                summary.Categories = GetResourcesCategorizedList(map);
                summary.CriticalResources = AnalyzeCriticalResources(map, allItems);
                return summary;
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Error generating resources summary: {ex}");
            }
            return new ResourcesSummaryDto();
        }

        private CriticalResourcesDto AnalyzeCriticalResources(Map map, List<Thing> allItems)
        {
            var critical = new CriticalResourcesDto();

            // Food analysis
            critical.FoodSummary = GetResourcesFoodSummary(map, allItems);
            var foodItems = allItems.Where(t => t.def.IsNutritionGivingIngestible).ToList();

            // Medicine analysis
            var medicineItems = allItems.Where(t => t.def.IsMedicine).ToList();
            critical.MedicineTotal = medicineItems.Sum(t => t.stackCount);

            // Weapon analysis
            var weapons = allItems.Where(t => t.def.IsWeapon).ToList();
            critical.WeaponCount = weapons.Count;
            critical.WeaponValue = weapons.Sum(t => t.MarketValue);

            return critical;
        }

        private ResourcesFoodSummaryDto GetResourcesFoodSummary(Map map, List<Thing> allItems)
        {
            ResourcesFoodSummaryDto summaryDto = new ResourcesFoodSummaryDto();

            var foodItems = allItems.Where(t => t.def.IsNutritionGivingIngestible).ToList();
            summaryDto.FoodTotal = foodItems.Sum(t => t.stackCount);


            float totalNutrition = map.resourceCounter.TotalHumanEdibleNutrition;
            int colonistCount = map.mapPawns.FreeColonistsSpawnedCount + map.mapPawns.PrisonersOfColonyCount;
            int daysOfFood = Mathf.FloorToInt(totalNutrition / colonistCount);

            summaryDto.TotalNutrition = foodItems.Sum(f => GetFoodNutrition(f));
            var dto = new ResourcesFoodSummaryDto
            {
                FoodTotal = allItems
                    .Count(t => t.def.IsNutritionGivingIngestible),
                TotalNutrition = map.resourceCounter.TotalHumanEdibleNutrition,
                MealsCount = allItems
                    .Count(t => t.def.IsNutritionGivingIngestible &&
                                t.def.ingestible.IsMeal),
                RawFoodCount = allItems
                    .Count(t => t.def.IsNutritionGivingIngestible &&
                                !t.def.ingestible.IsMeal),
                RotStatusInfo = GetFoodRotStatusInfo(map, allItems)
            };

            return summaryDto;
        }

        public RotStatusInfoDto GetFoodRotStatusInfo(Map map, List<Thing> allItems)
        {
            var dto = new RotStatusInfoDto();

            try
            {
                float foodRotatingSoon = 0f;
                float foodNotRotating = 0f;
                var soonRottingItems = new List<RottingFoodItemDto>();

                foreach (Thing thing in allItems)
                {
                    if (thing.def.IsNutritionGivingIngestible && !(thing is Corpse))
                    {
                        float daysUntilRot = 600f; // Infinite by default
                        CompRottable rottable = thing.TryGetComp<CompRottable>();

                        if (rottable != null && rottable.Active)
                        {
                            daysUntilRot = (float)DaysUntilRotCalculator.ApproxTicksUntilRot_AssumeTimePassesBy(
                                rottable, map.Tile) / 60000f;
                        }

                        float nutrition = thing.GetStatValue(StatDefOf.Nutrition) * thing.stackCount;

                        if (daysUntilRot < 1f) // Less than 1 day to rot
                        {
                            foodRotatingSoon += nutrition;

                            // Add to soon rotting items list
                            soonRottingItems.Add(new RottingFoodItemDto
                            {
                                ThingId = thing.thingIDNumber,
                                DefName = thing.def.defName,
                                Label = thing.Label,
                                StackCount = thing.stackCount,
                                Nutrition = nutrition,
                                DaysUntilRot = daysUntilRot,
                                HoursUntilRot = daysUntilRot * 24f,
                            });
                        }
                        else
                        {
                            foodNotRotating += nutrition;
                        }
                    }
                }

                dto.NutritionRotatingSoon = foodRotatingSoon;
                dto.NutritionNotRotating = foodNotRotating;
                dto.PercentageRotatingSoon = (foodRotatingSoon + foodNotRotating) > 0
                    ? foodRotatingSoon / (foodRotatingSoon + foodNotRotating)
                    : 0f;
                dto.SoonRottingItems = soonRottingItems.OrderBy(x => x.DaysUntilRot).ToList();
                dto.TotalSoonRottingItems = soonRottingItems.Count;
                dto.TotalSoonRottingStacks = soonRottingItems.Sum(x => x.StackCount);

                return dto;
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Error getting food rot status: {ex}");
                return dto;
            }
        }

        private float GetFoodNutrition(Thing food)
        {
            CompRottable comp = food.TryGetComp<CompRottable>();

            if (comp != null && comp.PropsRot.daysToRotStart > 0 && food.def.ingestible.HumanEdible)
            {
                return food.stackCount * food.def.GetStatValueAbstract(StatDefOf.Nutrition);
            }
            return 0f;
        }

        public StoragesSummaryDto StoragesSummary(Map map)
        {
            var analysis = new StoragesSummaryDto();

            try
            {
                var stockpiles = map.zoneManager.AllZones.OfType<Zone_Stockpile>().ToList();
                analysis.TotalStockpiles = stockpiles.Count;
                analysis.TotalCells = stockpiles.Sum(z => z.CellCount);
                analysis.UsedCells = stockpiles.Sum(z => z.Cells.Count(c => c.GetItemCount(map) > 0));

                if (analysis.TotalCells > 0)
                {
                    analysis.UtilizationPercent = (int)((double)analysis.UsedCells / analysis.TotalCells * 100);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Error analyzing storage: {ex}");
            }

            return analysis;
        }

        private List<ResourceCategoryDto> GetResourcesCategorizedList(Map map)
        {
            try
            {
                var categoryData = new Dictionary<ThingCategoryDef, (int count, double value)>();

                var rootCategories = DefDatabase<ThingCategoryDef>.AllDefs
                    .Where(cat => cat.resourceReadoutRoot);

                foreach (var category in rootCategories)
                {
                    int count = map.resourceCounter.GetCountIn(category);
                    double marketValue = 0.0;

                    foreach (var kvp in map.resourceCounter.AllCountedAmounts)
                    {
                        ThingDef thingDef = kvp.Key;
                        int thingCount = kvp.Value;

                        if (thingCount > 0 && category.ContainedInThisOrDescendant(thingDef))
                        {
                            marketValue += thingDef.BaseMarketValue * thingCount;
                        }
                    }

                    if (count > 0)
                    {
                        categoryData[category] = (count, marketValue);
                    }
                }

                return categoryData.Select(kvp => new ResourceCategoryDto
                {
                    Category = kvp.Key.LabelCap,
                    Count = kvp.Value.count,
                    MarketValue = kvp.Value.value
                }).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Error analyzing storage: {ex.Message}");
            }
            return new List<ResourceCategoryDto>();
        }
    }
}
