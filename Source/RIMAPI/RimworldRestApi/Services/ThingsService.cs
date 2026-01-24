
using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RIMAPI.Services;
using RimWorld;
using Verse;

namespace RIMAPI.Services
{
    class ThingsService : IThingsService
    {
        public ApiResult<ItemRecipesDto> GetItemRecipes(string defName)
        {
            try
            {
                // 1. Find the Item Definition
                ThingDef itemDef = DefDatabase<ThingDef>.GetNamed(defName, false);
                if (itemDef == null)
                {
                    return ApiResult<ItemRecipesDto>.Fail($"Item with DefName '{defName}' not found.");
                }

                var result = new ItemRecipesDto
                {
                    ItemDefName = itemDef.defName,
                    ItemLabel = itemDef.label,
                    Recipes = new List<ThingRecipeDto>()
                };

                // 2. Find relevant recipes
                var relevantRecipes = DefDatabase<RecipeDef>.AllDefsListForReading
                    .Where(r => r.products != null && r.products.Any(p => p.thingDef == itemDef));

                // 3. Pre-calculate the reverse lookup for "Produced At"
                // Many recipes (like Meals) are assigned TO benches, not the other way around.
                // We scan all buildings once to find which ones can produce which recipes.
                var recipeProducers = new Dictionary<RecipeDef, List<ThingDef>>();

                foreach (var building in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (building.AllRecipes != null)
                    {
                        foreach (var recipe in building.AllRecipes)
                        {
                            if (!recipeProducers.ContainsKey(recipe))
                                recipeProducers[recipe] = new List<ThingDef>();

                            if (!recipeProducers[recipe].Contains(building))
                                recipeProducers[recipe].Add(building);
                        }
                    }
                }

                foreach (var recipe in relevantRecipes)
                {
                    // FIX 1: Use WorkAmountTotal(null) to handle the "-1" case correctly
                    float realWorkAmount = recipe.WorkAmountTotal(null);

                    var recipeDto = new ThingRecipeDto
                    {
                        RecipeDefName = recipe.defName,
                        Label = recipe.label,
                        JobString = recipe.jobString,
                        WorkAmount = realWorkAmount,
                        WorkTimeSeconds = realWorkAmount / 60f,
                        ResearchPrerequisite = recipe.researchPrerequisite?.defName,
                        Ingredients = new List<RecipeIngredientDto>(),
                        ProducedAt = new List<RecipeProducerDto>(),
                        SkillRequirements = new List<RecipeSkillDto>()
                    };

                    // Ingredients Processing
                    foreach (var ing in recipe.ingredients)
                    {
                        var ingDto = new RecipeIngredientDto
                        {
                            Count = ing.GetBaseCount(),
                            Summary = ing.filter.Summary,
                            IsFixedItem = ing.filter.AllowedDefCount == 1,
                            // Limit to 20 to prevent massive JSON for "Any Meat" categories
                            AllowedDefNames = ing.filter.AllowedThingDefs.Select(t => t.defName).Take(20).ToList()
                        };
                        recipeDto.Ingredients.Add(ingDto);
                    }

                    // FIX 2: Populate Producers using the reverse lookup dictionary
                    // We also check the recipe's direct 'recipeUsers' list just in case
                    var producers = new HashSet<ThingDef>(); // Use HashSet to avoid duplicates

                    // A. Check direct link (Recipe -> User)
                    if (recipe.recipeUsers != null)
                    {
                        foreach (var user in recipe.recipeUsers) producers.Add(user);
                    }

                    // B. Check reverse link (Workbench -> Recipe)
                    if (recipeProducers.TryGetValue(recipe, out var benchDefs))
                    {
                        foreach (var bench in benchDefs) producers.Add(bench);
                    }

                    foreach (var producer in producers)
                    {
                        recipeDto.ProducedAt.Add(new RecipeProducerDto
                        {
                            DefName = producer.defName,
                            Label = producer.label
                        });
                    }

                    // Skills Processing
                    if (recipe.skillRequirements != null)
                    {
                        foreach (var skill in recipe.skillRequirements)
                        {
                            recipeDto.SkillRequirements.Add(new RecipeSkillDto
                            {
                                Skill = skill.skill.defName,
                                MinLevel = skill.minLevel
                            });
                        }
                    }

                    result.Recipes.Add(recipeDto);
                }

                return ApiResult<ItemRecipesDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResult<ItemRecipesDto>.Fail(ex.Message);
            }
        }

        public ApiResult<ThingSourcesDto> GetItemSources(string defName)
        {
            try
            {
                // 1. Find the Item
                ThingDef itemDef = DefDatabase<ThingDef>.GetNamed(defName, false);
                if (itemDef == null)
                {
                    return ApiResult<ThingSourcesDto>.Fail($"Item '{defName}' not found.");
                }

                var result = new ThingSourcesDto
                {
                    DefName = itemDef.defName,
                    Label = itemDef.label,

                    CraftingRecipes = new List<string>(),
                    HarvestedFrom = new List<string>(),
                    MinedFrom = new List<string>(),
                    TradeTags = new List<string>()
                };

                result.ThingCategories = itemDef.thingCategories?
                    .Select(c => c.defName)
                    .ToList() ?? new List<string>();

                // 2. CHECK CRAFTING
                // Check if any recipe produces this item
                var recipes = DefDatabase<RecipeDef>.AllDefsListForReading
                    .Where(r => r.products != null && r.products.Any(p => p.thingDef == itemDef));

                foreach (var r in recipes)
                {
                    result.CraftingRecipes.Add(r.label);
                }
                result.CanCraft = result.CraftingRecipes.Count > 0;

                // 3. CHECK MINING
                // Scan all buildings (rocks/ores) to see if their 'mineableThing' is this item
                var mineables = DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(t => t.building != null && t.building.mineableThing == itemDef);

                foreach (var m in mineables)
                {
                    result.MinedFrom.Add(m.label);
                }
                result.CanMine = result.MinedFrom.Count > 0;

                // 4. CHECK HARVESTING (Plants)
                // Scan all plants to see if their 'harvestedThingDef' is this item
                var plants = DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(t => t.plant != null && t.plant.harvestedThingDef == itemDef);

                foreach (var p in plants)
                {
                    result.HarvestedFrom.Add(p.label);
                }
                result.CanHarvest = result.HarvestedFrom.Count > 0;

                // 5. CHECK BUTCHERING (Animals)
                // Check if item is defined as meat or leather
                // (We don't list every animal because that list is huge)
                if (itemDef.IsMeat || itemDef.IsLeather)
                {
                    result.CanButcher = true;
                    // Optionally add a generic note
                    result.HarvestedFrom.Add("Various animals (Butchery)");
                }

                // 6. CHECK TRADE
                // Tradeability.All or Tradeability.Sellable means it appears in trade.
                // We also grab the tags to hint at WHICH trader type (e.g. "Exotic", "Food")
                if (itemDef.tradeability != Tradeability.None)
                {
                    result.CanTrade = true;
                    if (itemDef.tradeTags != null)
                    {
                        result.TradeTags.AddRange(itemDef.tradeTags);
                    }
                }

                return ApiResult<ThingSourcesDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResult<ThingSourcesDto>.Fail(ex.Message);
            }
        }
    }
}