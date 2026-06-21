using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class LearningConceptDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string HelpText { get; set; }
        public float KnowledgeProgress { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class LearningConceptsListDto
    {
        public List<string> Defs { get; set; }
    }

    public class LearningConceptMarkDto
    {
        public string DefName { get; set; }
        public bool IsCompleted { get; set; }
    }
}