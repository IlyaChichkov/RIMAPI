using System;

namespace RIMAPI.Models
{
    public class FactionsDto
    {
        public string Def { get; set; }
        public string Name { get; set; }
        public bool IsPlayer { get; set; }
        public string Relation { get; set; }
        public int Goodwill { get; set; }
    }
}
