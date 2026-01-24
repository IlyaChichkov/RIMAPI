

using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class ItemRecipesDto
    {
        public string ItemDefName { get; set; }
        public string ItemLabel { get; set; }
        public List<ThingRecipeDto> Recipes { get; set; }
    }

    public class ThingRecipeDto
    {
        public string RecipeDefName { get; set; }
        public string Label { get; set; } // e.g. "Cook simple meal"
        public string JobString { get; set; } // e.g. "Cooking simple meal."
        public float WorkAmount { get; set; }
        public float WorkTimeSeconds { get; set; } // Calculated (WorkAmount / 60)

        public List<RecipeIngredientDto> Ingredients { get; set; }
        public List<RecipeProducerDto> ProducedAt { get; set; }
        public List<RecipeSkillDto> SkillRequirements { get; set; }

        public string ResearchPrerequisite { get; set; }
    }

    public class RecipeIngredientDto
    {
        public string Summary { get; set; } // e.g. "Raw food", "Steel"
        public float Count { get; set; }
        public bool IsFixedItem { get; set; } // True if only 1 specific item is allowed
        public List<string> AllowedDefNames { get; set; } // List of valid items for this slot
    }

    public class RecipeProducerDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
    }

    public class RecipeSkillDto
    {
        public string Skill { get; set; }
        public int MinLevel { get; set; }
    }
}