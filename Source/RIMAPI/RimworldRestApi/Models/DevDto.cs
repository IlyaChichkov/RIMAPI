using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class DebugConsoleRequest
    {
        public string Action { get; set; }
        public string Message { get; set; }
    }

    public class StuffColorRequest
    {
        public string Name { get; set; }
        public string Hex { get; set; }
    }

    public class MaterialsAtlasList
    {
        public List<string> Materials { get; set; }
    }

    public class EndpointDto
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string[] Tags { get; set; }
        public bool IsDeprecated { get; set; }
    }

    public class EndpointListDto
    {
        public List<EndpointDto> Endpoints { get; set; }
    }
}
