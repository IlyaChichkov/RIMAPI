using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class IntRangeDto
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public IntRangeDto() { }

        public IntRangeDto(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    public class BillDto
    {
        public int LoadId { get; set; }
        public string RecipeDefName { get; set; }
        public string RecipeLabel { get; set; }
        public bool Suspended { get; set; }
        public bool Paused { get; set; }
        public string RepeatMode { get; set; }
        public int RepeatCount { get; set; }
        public int TargetCount { get; set; }
        public string StoreMode { get; set; }
        public bool PauseWhenSatisfied { get; set; }
        public int UnpauseWhenYouHave { get; set; }
        public bool IncludeEquipped { get; set; }
        public bool IncludeTainted { get; set; }
        public FloatRange HpRange { get; set; }
        public IntRangeDto QualityRange { get; set; }
        public bool LimitToAllowedStuff { get; set; }
        public float IngredientSearchRadius { get; set; }
        public IntRangeDto AllowedSkillRange { get; set; }
        public int? PawnRestrictionId { get; set; }
        public string PlayerCustomName { get; set; }
        public int? SlotGroupId { get; set; }
    }

    public class CreateBillRequest
    {
        public string RecipeDefName { get; set; }
        public string RepeatMode { get; set; }
        public int? RepeatCount { get; set; }
        public int? TargetCount { get; set; }
        public string StoreMode { get; set; }
        public bool? Suspended { get; set; }
        public bool? PauseWhenSatisfied { get; set; }
        public int? UnpauseWhenYouHave { get; set; }
        public bool? IncludeEquipped { get; set; }
        public bool? IncludeTainted { get; set; }
        public FloatRange HpRange { get; set; }
        public IntRangeDto QualityRange { get; set; }
        public bool? LimitToAllowedStuff { get; set; }
        public float? IngredientSearchRadius { get; set; }
        public IntRangeDto AllowedSkillRange { get; set; }
        public int? PawnRestrictionId { get; set; }
        public string PlayerCustomName { get; set; }
    }

    public class UpdateBillRequest
    {
        public string RepeatMode { get; set; }
        public int? RepeatCount { get; set; }
        public int? TargetCount { get; set; }
        public string StoreMode { get; set; }
        public bool? Suspended { get; set; }
        public bool? PauseWhenSatisfied { get; set; }
        public int? UnpauseWhenYouHave { get; set; }
        public bool? IncludeEquipped { get; set; }
        public bool? IncludeTainted { get; set; }
        public FloatRange HpRange { get; set; }
        public IntRangeDto QualityRange { get; set; }
        public bool? LimitToAllowedStuff { get; set; }
        public float? IngredientSearchRadius { get; set; }
        public IntRangeDto AllowedSkillRange { get; set; }
        public int? PawnRestrictionId { get; set; }
        public string PlayerCustomName { get; set; }
    }

    public class BillReorderRequest
    {
        public int Offset { get; set; }
    }

    public class BillSuspendRequest
    {
        public bool Suspended { get; set; }
    }

    public class RecipeProductDto
    {
        public string ThingDef { get; set; }
        public int Count { get; set; }
    }

    public class BillRecipeIngredientDto
    {
        public string FilterLabel { get; set; }
        public float Count { get; set; }
    }

    public class RecipeDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float WorkAmount { get; set; }
        public string WorkSkill { get; set; }
        public List<RecipeProductDto> Products { get; set; } = new List<RecipeProductDto>();
        public List<BillRecipeIngredientDto> Ingredients { get; set; } = new List<BillRecipeIngredientDto>();
    }

    public class WorkTableDto
    {
        public int Id { get; set; }
        public string ThingDef { get; set; }
        public string Label { get; set; }
        public PositionDto Position { get; set; }
        public int BillsCount { get; set; }
    }
}