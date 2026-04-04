
using System.Collections.Generic;

namespace RIMAPI.Models.UI
{
    public class AlertDto
    {
        public string Label { get; set; }
        public string Explanation { get; set; }

        // e.g., "Critical", "High", "Medium"
        public string Priority { get; set; }
        public List<int> Targets { get; set; }
        public List<string> Cells { get; set; }
    }
}