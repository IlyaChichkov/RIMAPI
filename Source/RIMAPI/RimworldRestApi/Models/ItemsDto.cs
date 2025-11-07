using System;

namespace RimworldRestApi.Models
{
    public class InventoryThingDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StackCount { get; set; }
    }

    public class MapThingDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Def { get; set; }
        public PositionDto Position { get; set; }
        public bool IsForbidden { get; set; }
    }
}