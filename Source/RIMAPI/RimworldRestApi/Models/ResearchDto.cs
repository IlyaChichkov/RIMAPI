using System.Collections.Generic;

namespace RIMAPI.Models
{
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
        public bool CanStartNow { get; set; }
        public bool PlayerHasAnyAppropriateResearchBench { get; set; }
        public int RequiredAnalyzedThingCount { get; set; }
        public int AnalyzedThingsCompleted { get; set; }
        public string TechLevel { get; set; }

        public List<string> Prerequisites { get; set; } = new List<string>();
        public List<string> HiddenPrerequisites { get; set; } = new List<string>();
        public List<string> RequiredByThis { get; set; } = new List<string>();
        public float ProgressPercent { get; set; }
    }

    public class ResearchSummaryDto
    {
        public int FinishedProjectsCount { get; set; }
        public int TotalProjectsCount { get; set; }
        public int AvailableProjectsCount { get; set; }
        public Dictionary<string, ResearchCategoryDto> ByTechLevel { get; set; }
        public Dictionary<string, ResearchCategoryDto> ByTab { get; set; }
    }

    public class ResearchCategoryDto
    {
        public int Finished { get; set; }
        public int Total { get; set; }
        public float PercentComplete => Total > 0 ? Finished / (float)Total * 100 : 0;
        public List<string> Projects { get; set; }
    }
}
