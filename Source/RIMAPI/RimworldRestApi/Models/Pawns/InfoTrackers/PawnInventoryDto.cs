using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class PawnInventoryDto
    {
        public List<ThingDto> Items { get; set; }
        public List<ThingDto> Apparels { get; set; }
        public List<ThingDto> Equipment { get; set; }
    }
}