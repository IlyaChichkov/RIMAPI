using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RIMAPI.Models
{
    public class ThingDto
    {
        public int ThingId { get; set; }
        public string DefName { get; set; }
        public string Label { get; set; }
        public List<string> Categories { get; set; }
        public PositionDto Position { get; set; }
        public int Rotation { get; set; }
        public PositionDto Size { get; set; }
        public int StackCount { get; set; }
        public double MarketValue { get; set; }
        public bool IsForbidden { get; set; }
        public int Quality { get; set; }
        public string StuffDefName { get; set; }
        public int HitPoints { get; set; }
        public int MaxHitPoints { get; set; }
        public string Description { get; set; }

        public static ThingDto ToDto(Thing thing)
        {
            var dto = new ThingDto
            {
                ThingId = thing.thingIDNumber,
                DefName = thing.def.defName,
                Label = thing.Label,
                Categories = thing.def.thingCategories?.Select(c => c.defName).ToList() ?? new List<string>(),
                Position = new PositionDto
                {
                    X = thing.Position.x,
                    Y = thing.Position.y,
                    Z = thing.Position.z,
                },
                StackCount = thing.stackCount,
                MarketValue = thing.MarketValue,
                IsForbidden = thing.IsForbidden(Faction.OfPlayer),
                HitPoints = thing.HitPoints,
                MaxHitPoints = thing.MaxHitPoints,
                Rotation = thing.Rotation.AsInt,

                // Map the new fields
                StuffDefName = thing.Stuff?.defName,
                Description = thing.DescriptionDetailed
            };

            // Safely get Quality
            var qualityComp = thing.TryGetComp<CompQuality>();
            if (qualityComp != null)
            {
                dto.Quality = (int)qualityComp.Quality;
            }

            return dto;
        }
    }

    public class ThingSourcesDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public List<string> ThingCategories { get; set; }

        // -- Acquisition Flags --
        public bool CanCraft { get; set; }
        public bool CanTrade { get; set; }
        public bool CanHarvest { get; set; } // Plants
        public bool CanMine { get; set; }    // Ores
        public bool CanButcher { get; set; } // Meat/Leather

        // -- Detailed Sources --
        public List<string> CraftingRecipes { get; set; } // e.g. "Smelt metal from slag"
        public List<string> HarvestedFrom { get; set; }   // e.g. "Oak tree", "Corn plant"
        public List<string> MinedFrom { get; set; }       // e.g. "Compacted machinery"
        public List<string> TradeTags { get; set; }       // e.g. "ExoticMisc", "ResourcesRaw"
    }
}