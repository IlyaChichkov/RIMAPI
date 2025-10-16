

using System.Collections.Generic;

namespace RimworldRestApi.Models
{
    public class ResearchProgressDto
    {
        public string CurrentProject { get; set; }
        public string Label { get; set; }
        public float Progress { get; set; }
    }

    public class ResearchFinishedDto
    {
        public List<string> FinishedProjects { get; set; } = new List<string>();
    }

    public class ResearchTreeDto
    {
        public List<ResearchProjectDto> Projects { get; set; } = new List<ResearchProjectDto>();
    }

    public class ResearchProjectDto
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public int Progress { get; set; }
        public int ResearchPoints { get; set; }
        public string Description { get; set; }
        public bool IsFinished { get; set; }
        public bool IsAvailable { get; set; }
        public string TechLevel { get; set; }
    }
}