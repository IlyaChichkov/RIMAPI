

using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class ConfigureModsRequestDto
    {
        public List<string> PackageIds { get; set; }
        public bool RestartGame { get; set; } = false;
    }

    public class ModInfoDto
    {
        public string Name { get; set; }
        public string PackageId { get; set; }
        public int LoadOrder { get; set; }
    }
}
