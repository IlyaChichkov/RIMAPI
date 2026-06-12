

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
        public string Author { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Version { get; set; }
        public List<string> SupportedVersions { get; set; }
        public string RootDir { get; set; }
    }
}
