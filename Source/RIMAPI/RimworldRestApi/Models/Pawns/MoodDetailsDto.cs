using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class MoodDetailsDto
    {
        public bool Available { get; set; }

        public float CurrentLevel { get; set; }

        public float? TargetLevel { get; set; }

        public float TotalThoughtOffset { get; set; }

        public List<MoodFactorDto> Factors { get; set; } = new List<MoodFactorDto>();
    }

    public class MoodFactorDto
    {
        public string DefName { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public float MoodOffset { get; set; }

        public int StackCount { get; set; }

        public int StageIndex { get; set; }

        public string Kind { get; set; }

        public float StackedEffectMultiplier { get; set; }
    }
}
