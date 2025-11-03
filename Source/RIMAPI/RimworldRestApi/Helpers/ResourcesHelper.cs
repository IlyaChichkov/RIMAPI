using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimworldRestApi.Core;
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
                DebugLogging.Error($"[RIMAPI] Error generating resources summary: {ex}");
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
                                rottable, MapHelper.GetMapTileId(map)) / 60000f;
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
                DebugLogging.Error($"[RIMAPI] Error getting food rot status: {ex}");
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
                DebugLogging.Error($"[RIMAPI] Error analyzing storage: {ex}");
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
                DebugLogging.Error($"[RIMAPI] Error analyzing storage: {ex.Message}");
            }
            return new List<ResourceCategoryDto>();
        }

        public List<ISlotGroupParent> GetAllStorageLocations(Map map)
        {
            List<ISlotGroupParent> storageLocations = new List<ISlotGroupParent>();

            // Get all storage buildings  
            List<Building> allBuildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < allBuildings.Count; i++)
            {
                Building_Storage storage = allBuildings[i] as Building_Storage;
                if (storage != null)
                {
                    storageLocations.Add(storage);
                }
            }

            // Get all stockpile zones  
            List<Zone> allZones = map.zoneManager.AllZones;
            for (int i = 0; i < allZones.Count; i++)
            {
                Zone_Stockpile stockpile = allZones[i] as Zone_Stockpile;
                if (stockpile != null)
                {
                    storageLocations.Add(stockpile);
                }
            }

            return storageLocations;
        }

        public Dictionary<string, List<ResourceItemDto>> GetStoredItemsByCategory(
            List<ISlotGroupParent> storageLocations)
        {
            if (storageLocations == null || storageLocations.Count == 0)
                return new Dictionary<string, List<ResourceItemDto>>();

            var itemsByCategory = new Dictionary<string, List<ResourceItemDto>>();

            foreach (var storage in storageLocations)
            {
                var slotGroup = storage.GetSlotGroup();
                if (slotGroup == null) continue;

                foreach (var thing in slotGroup.HeldThings)
                {
                    var category = GetPrimaryCategoryForThing(thing.def);
                    if (category == null) continue;

                    var categoryLabel = category.defName;

                    // Get or create the list for this category
                    if (!itemsByCategory.TryGetValue(categoryLabel, out var categoryList))
                    {
                        categoryList = new List<ResourceItemDto>();
                        itemsByCategory[categoryLabel] = categoryList;
                    }

                    categoryList.Add(CreateResourceItemDto(thing));
                }
            }

            return itemsByCategory;
        }

        public List<ResourceItemDto> GetStoredItemsListByCategory(
            List<ISlotGroupParent> storageLocations,
            string categoryDef)
        {
            if (storageLocations == null || storageLocations.Count == 0)
                return new List<ResourceItemDto>();

            var itemsByCategory = new List<ResourceItemDto>();

            foreach (var storage in storageLocations)
            {
                var slotGroup = storage.GetSlotGroup();
                if (slotGroup == null) continue;

                foreach (var thing in slotGroup.HeldThings)
                {
                    if (!IsThingInCategory(thing, categoryDef)) continue;

                    var dto = CreateResourceItemDto(thing);
                    itemsByCategory.Add(dto);
                }
            }

            return itemsByCategory;
        }

        private bool IsThingInCategory(Thing thing, string categoryDef)
        {
            var categories = thing.def.thingCategories;
            return categories != null && categories.Any(c => c.defName == TransformToPascalCase(categoryDef));
        }

        private ResourceItemDto CreateResourceItemDto(Thing thing)
        {
            var dto = new ResourceItemDto
            {
                ThingId = thing.thingIDNumber,
                DefName = thing.def.defName,
                Label = thing.Label,
                Categories = thing.def.thingCategories?.Select(c => c.defName).ToList() ?? new List<string>(),
                Position = new PositionDto
                {
                    X = thing.Position.x,
                    Y = thing.Position.y,
                    Z = thing.Position.z
                },
                StackCount = thing.stackCount,
                MarketValue = thing.MarketValue,
                IsForbidden = thing.IsForbidden(Faction.OfPlayer)
            };

            dto.Quality = -1;
            if (thing.TryGetQuality(out QualityCategory quality))
            {
                dto.Quality = (int)quality;
            }

            // Set hit points if applicable
            if (thing.def.useHitPoints)
            {
                dto.HitPoints = thing.HitPoints;
                dto.MaxHitPoints = thing.MaxHitPoints;
            }

            return dto;
        }

        private static ThingCategoryDef GetPrimaryCategoryForThing(ThingDef thingDef)
        {
            // Get the first (most specific) category from thingCategories list  
            if (thingDef.thingCategories != null && thingDef.thingCategories.Count > 0)
            {
                return thingDef.thingCategories[0];
            }
            return null;
        }

        public string TransformToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.Replace('_', ' ')).Replace(" ", "");
        }

    }
}
