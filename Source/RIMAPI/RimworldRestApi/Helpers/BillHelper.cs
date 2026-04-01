using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RIMAPI.Core;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Helpers
{
    public static class BillHelper
    {
        private static readonly FieldInfo LoadIdField = typeof(Bill).GetField("loadID", BindingFlags.Instance | BindingFlags.NonPublic);

        public static BillDto ToDto(Bill_Production bill)
        {
            if (bill == null)
                return null;

            try
            {
                if (LoadIdField == null)
                    Core.LogApi.Warning("[BillHelper] Failed to access loadID field via reflection");

                int loadId = LoadIdField != null ? (int)LoadIdField.GetValue(bill) : 0;

                return new BillDto
                {
                    LoadId = loadId,
                    RecipeDefName = bill.recipe.defName,
                    RecipeLabel = bill.recipe.label,
                    Suspended = bill.suspended,
                    Paused = bill.paused,
                    RepeatMode = bill.repeatMode.defName,
                    RepeatCount = bill.repeatCount,
                    TargetCount = bill.targetCount,
                    StoreMode = bill.GetStoreMode().defName,
                    PauseWhenSatisfied = bill.pauseWhenSatisfied,
                    UnpauseWhenYouHave = bill.unpauseWhenYouHave,
                    IncludeEquipped = bill.includeEquipped,
                    IncludeTainted = bill.includeTainted,
                    HpRange = new Models.FloatRange(bill.hpRange.min, bill.hpRange.max),
                    QualityRange = new IntRangeDto((int)bill.qualityRange.min, (int)bill.qualityRange.max),
                    LimitToAllowedStuff = bill.limitToAllowedStuff,
                    IngredientSearchRadius = bill.ingredientSearchRadius,
                    AllowedSkillRange = new IntRangeDto(bill.allowedSkillRange.min, bill.allowedSkillRange.max),
                    PawnRestrictionId = bill.PawnRestriction?.thingIDNumber,
                    PlayerCustomName = bill.RenamableLabel,
                    SlotGroupId = null,
                    AllowedMaterials = bill.ingredientFilter?.AllowedThingDefs?.Select(d => d.defName).ToList() ?? new List<string>(),
                    AvailableMaterials = bill.recipe.fixedIngredientFilter?.AllowedThingDefs?.Select(d => d.defName).ToList() ?? new List<string>(),
                };
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"[BillHelper] Error converting Bill to DTO: {ex.Message}");
                return null;
            }
        }

        public static RecipeDto ToRecipeDto(RecipeDef recipe)
        {
            if (recipe == null)
                return null;

            var dto = new RecipeDto
            {
                DefName = recipe.defName,
                Label = recipe.label,
                Description = recipe.description,
                WorkAmount = recipe.workAmount,
                WorkSkill = recipe.workSkill?.label,
            };

            if (recipe.products != null)
            {
                dto.Products = recipe.products
                    .Select(p => new RecipeProductDto
                    {
                        ThingDef = p.thingDef.defName,
                        Count = p.count,
                    })
                    .ToList();
            }

            if (recipe.ingredients != null)
            {
                dto.Ingredients = recipe.ingredients
                    .Select(i => new BillRecipeIngredientDto
                    {
                        FilterLabel = i.filter.Summary,
                        Count = i.GetBaseCount(),
                    })
                    .ToList();
            }

            return dto;
        }

        public static WorkTableDto ToWorkTableDto(Building_WorkTable wt)
        {
            if (wt == null)
                return null;

            return new WorkTableDto
            {
                Id = wt.thingIDNumber,
                ThingDef = wt.def.defName,
                Label = wt.LabelShort,
                Position = new PositionDto
                {
                    X = wt.Position.x,
                    Y = wt.Position.y,
                    Z = wt.Position.z,
                },
                BillsCount = wt.BillStack.Bills.Count,
            };
        }
    }
}
