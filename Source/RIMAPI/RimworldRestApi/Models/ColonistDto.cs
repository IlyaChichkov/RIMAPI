using System.Collections.Generic;

namespace RimworldRestApi.Models
{
    public class ColonistDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public float Health { get; set; }
        public float Mood { get; set; }
        public PositionDto Position { get; set; }
    }

    public class PositionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public class ColonistDetailedDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public PositionDto Position { get; set; }
        public float Mood { get; set; } // Percentage (0-100)
        public float Health { get; set; } // Percentage (0-1)
        public List<HediffDto> Hediffs { get; set; }
        public string CurrentJob { get; set; }
        public List<string> Traits { get; set; }
        public List<WorkPriorityDto> WorkPriorities { get; set; }
    }

    public class HediffDto
    {
        public string Part { get; set; }
        public string Label { get; set; }
    }

    public class WorkPriorityDto
    {
        public string WorkType { get; set; }
        public int Priority { get; set; }
    }
}
