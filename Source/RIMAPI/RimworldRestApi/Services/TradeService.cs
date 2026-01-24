
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RIMAPI.Services;
using RimWorld;
using Verse;


namespace RIMAPI.Services
{
    class TradeService : ITradeService
    {
        public ApiResult<List<TraderKindDto>> GetAllTraderDefs()
        {
            try
            {
                var results = new List<TraderKindDto>();

                foreach (var def in DefDatabase<TraderKindDef>.AllDefsListForReading)
                {
                    var dto = new TraderKindDto
                    {
                        DefName = def.defName,
                        Label = def.label ?? def.defName,
                        Orbital = def.orbital,
                        Visitor = !def.orbital,
                        Commonality = def.commonality,
                        // Initialize lists only as needed below to save allocation
                        Items = new List<StockRuleDto>(),
                        Categories = new List<StockRuleDto>(),
                        Tags = new List<StockRuleDto>(),
                        Special = new List<StockRuleDto>()
                    };

                    foreach (var gen in def.stockGenerators)
                    {
                        var traverse = Traverse.Create(gen);

                        // 1. FIX COUNT: If range is effectively zero, make it null
                        string countRange = null;
                        if (gen.countRange.max > 0)
                        {
                            countRange = $"{gen.countRange.min}~{gen.countRange.max}";
                        }

                        string priceMode = gen.price == PriceType.Normal ? null : gen.price.ToString();

                        // Most generators allow buying, but strict "Buy" generators usually imply 
                        // they ONLY buy this and don't sell it.
                        // However, for the API, "Buys=true" is the important part.
                        bool buys = true;

                        StockRuleDto CreateRule(string name) => new StockRuleDto
                        {
                            Name = name,
                            Count = countRange, // Will be null if 0~0
                            Price = priceMode,
                            Buys = buys
                        };

                        // --- STANDARD GENERATORS ---
                        if (gen is StockGenerator_SingleDef)
                        {
                            var thingDef = traverse.Field("thingDef").GetValue<ThingDef>();
                            if (thingDef != null) dto.Items.Add(CreateRule(thingDef.label));
                        }
                        else if (gen is StockGenerator_MultiDef)
                        {
                            var thingDefs = traverse.Field("thingDefs").GetValue<List<ThingDef>>();
                            if (thingDefs != null)
                                foreach (var t in thingDefs) dto.Items.Add(CreateRule(t.label));
                        }
                        else if (gen is StockGenerator_Category)
                        {
                            var catDef = traverse.Field("categoryDef").GetValue<ThingCategoryDef>();
                            if (catDef != null) dto.Categories.Add(CreateRule(catDef.label));
                        }
                        else if (gen is StockGenerator_Tag)
                        {
                            var tag = traverse.Field("tradeTag").GetValue<string>();
                            if (!string.IsNullOrEmpty(tag)) dto.Tags.Add(CreateRule(tag));
                        }

                        // --- SPECIAL CASES (Enhanced Reflection) ---
                        else
                        {
                            string specialName;

                            if (gen is StockGenerator_Animals)
                            {
                                specialName = "Animals";
                            }
                            else if (gen is StockGenerator_BuySlaves)
                            {
                                specialName = "Slaves";
                            }
                            else if (gen is StockGenerator_Techprints)
                            {
                                specialName = "Techprints";
                            }
                            else if (gen is StockGenerator_ReinforcedBarrels)
                            {
                                specialName = "ReinforcedBarrels";
                            }
                            // FIX: Extract the tag from 'BuyTradeTag'
                            else if (gen.GetType().Name == "StockGenerator_BuyTradeTag")
                            {
                                var tag = traverse.Field("tag").GetValue<string>();
                                specialName = !string.IsNullOrEmpty(tag) ? $"Buys: {tag}" : "Buys: Tag";
                                countRange = null; // Explicitly ensure count is hidden for buy-only
                            }
                            // FIX: Extract the tag from 'MarketValue'
                            else if (gen.GetType().Name == "StockGenerator_MarketValue")
                            {
                                // MarketValue generators typically generate items of a specific tag 
                                // that fall within a value range.
                                var tag = traverse.Field("tradeTag").GetValue<string>();
                                specialName = !string.IsNullOrEmpty(tag) ? $"Random: {tag}" : "Random: Value";
                            }
                            else
                            {
                                specialName = gen.GetType().Name.Replace("StockGenerator_", "");
                            }

                            // Re-create rule to ensure updated specialName/countRange is used
                            var rule = CreateRule(specialName);
                            rule.Count = countRange; // Update count in case we forced it to null above
                            dto.Special.Add(rule);
                        }
                    }

                    // Cleanup: Set empty lists to null so they aren't serialized
                    if (dto.Items.Count == 0) dto.Items = null;
                    if (dto.Categories.Count == 0) dto.Categories = null;
                    if (dto.Tags.Count == 0) dto.Tags = null;
                    if (dto.Special.Count == 0) dto.Special = null;

                    results.Add(dto);
                }

                return ApiResult<List<TraderKindDto>>.Ok(results);
            }
            catch (Exception ex)
            {
                return ApiResult<List<TraderKindDto>>.Fail(ex.Message);
            }
        }
    }
}