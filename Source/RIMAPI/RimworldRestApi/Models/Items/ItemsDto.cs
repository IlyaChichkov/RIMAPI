using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RIMAPI.Models
{
    public class ThingDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ThingClass { get; set; }
        public Dictionary<string, float> StatBase { get; set; }
        public List<ThingCostDto> CostList { get; set; }
        public bool IsWeapon { get; set; }
        public bool IsApparel { get; set; }
        public bool IsItem { get; set; }
        public bool IsPawn { get; set; }
        public bool IsPlant { get; set; }
        public bool IsBuilding { get; set; }
        public bool IsMedicine { get; set; }
        public bool IsDrug { get; set; }
        public float MarketValue { get; set; }
        public float Mass { get; set; }
        public float MaxHitPoints { get; set; }
        public float Flammability { get; set; }
        public int StackLimit { get; set; }
        public float Nutrition { get; set; }
        public float WorkToMake { get; set; }
        public float WorkToBuild { get; set; }
        public float Beauty { get; set; }
        public string TechLevel { get; set; }
        public List<string> TradeTags { get; set; }
        public List<string> StuffCategories { get; set; }
        public float MaxHealth { get; set; }
        public float ArmorRating_Sharp { get; set; }
        public float ArmorRating_Blunt { get; set; }
        public float ArmorRating_Heat { get; set; }
        public float Insulation_Cold { get; set; }
        public float Insulation_Heat { get; set; }

        public static ThingDefDto ToDto(ThingDef thing)
        {
            return new ThingDefDto
            {
                DefName = thing.defName,
                Label = thing.label,
                Description = thing.description,
                Category = thing.category.ToString(),
                ThingClass = thing.thingClass?.Name,
                StatBase = thing.statBases?.ToDictionary(s => s.stat?.defName, s => s.value),
                CostList = thing
                                    .costList?.Select(c => new ThingCostDto
                                    {
                                        ThingDef = c.thingDef?.defName,
                                        Count = c.count,
                                    })
                                    .ToList(),
                IsWeapon = thing.IsWeapon,
                IsApparel = thing.IsApparel,
                IsItem = thing.category == ThingCategory.Item,
                IsPawn = thing.category == ThingCategory.Pawn,
                IsPlant = thing.category == ThingCategory.Plant,
                IsBuilding = thing.category == ThingCategory.Building,
                IsMedicine = thing.IsMedicine,
                IsDrug = thing.IsDrug,
                MarketValue = thing.BaseMarketValue,
                Mass = thing.GetStatValueAbstract(StatDefOf.Mass),
                MaxHitPoints = thing.GetStatValueAbstract(StatDefOf.MaxHitPoints),
                Flammability = thing.GetStatValueAbstract(StatDefOf.Flammability),
                StackLimit = thing.stackLimit,
                Nutrition = thing.GetStatValueAbstract(StatDefOf.Nutrition),
                WorkToMake = thing.GetStatValueAbstract(StatDefOf.WorkToMake),
                WorkToBuild = thing.GetStatValueAbstract(StatDefOf.WorkToBuild),
                Beauty = thing.GetStatValueAbstract(StatDefOf.Beauty),
            };
        }
    }

    public class SpawnItemRequestDto
    {
        public string defName;      // "Gun_Revolver"
        public string stuffDefName; // "Steel" (Optional, can be null)
        public string quality;      // "Normal", "Good", "Legendary" (Optional)
        public int amount;
        public int x;
        public int z;
    }
}
