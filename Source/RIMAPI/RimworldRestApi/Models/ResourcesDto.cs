using System.Collections.Generic;

namespace RimworldRestApi.Models
{
    public class ResourcesSummaryDto
    {
        public int TotalItems { get; set; }
        public double TotalMarketValue { get; set; }
        public string LastUpdated { get; set; }
        public List<ResourceCategoryDto> Categories { get; set; }
        public CriticalResourcesDto CriticalResources { get; set; }
    }

    public class StoragesSummaryDto
    {
        public int TotalStockpiles { get; set; }
        public int TotalCells { get; set; }
        public int UsedCells { get; set; }
        public int UtilizationPercent { get; set; }
    }

    public class ResourceCategoryDto
    {
        public string Category { get; set; }
        public int Count { get; set; }
        public double MarketValue { get; set; }
    }

    public class ResourceItemDto
    {
        public int ThingId { get; set; }
        public string DefName { get; set; }
        public string Label { get; set; }
        public int StackCount { get; set; }
        public double MarketValue { get; set; }
        public bool IsForbidden { get; set; }
        public string Quality { get; set; }
        public string HitPoints { get; set; }
    }

    public class CriticalResourcesDto
    {
        public ResourcesFoodSummaryDto FoodSummary { get; set; }
        public int MedicineTotal { get; set; }
        public int WeaponCount { get; set; }
        public double WeaponValue { get; set; }
    }

    public class ResourcesFoodSummaryDto
    {
        public int FoodTotal { get; set; }
        public float TotalNutrition { get; set; }
        public int MealsCount { get; set; }
        public int RawFoodCount { get; set; }
        public RotStatusInfoDto RotStatusInfo { get; set; }
    }

    public class RotStatusInfoDto
    {
        public float NutritionRotatingSoon { get; set; }
        public float NutritionNotRotating { get; set; }
        public float PercentageRotatingSoon { get; set; }
        public List<RottingFoodItemDto> SoonRottingItems { get; set; } = new List<RottingFoodItemDto>();
        public int TotalSoonRottingItems { get; set; }
        public int TotalSoonRottingStacks { get; set; }
    }

    public class RottingFoodItemDto
    {
        public int ThingId { get; set; }
        public string DefName { get; set; }
        public string Label { get; set; }
        public int StackCount { get; set; }
        public float Nutrition { get; set; }
        public float DaysUntilRot { get; set; }
        public float HoursUntilRot { get; set; }
    }
}