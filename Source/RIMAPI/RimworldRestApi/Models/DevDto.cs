using System;
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class StuffColorRequest
    {
        public string Name { get; set; }
        public string Hex { get; set; }
    }

    public class MaterialsAtlasList
    {
        public List<string> Materials { get; set; }
    }
}
