using System;

namespace RimworldRestApi.Models
{
    public class FactionsDto
    {
        public string Name { get; set; }
        public string Def { get; set; }
        public bool IsPlayer { get; set; }
        public string Relation { get; set; }
        public int Goodwill { get; set; }
    }
}