using System;

namespace RIMAPI.Models
{
    public class GameStateDto
    {
        public int GameTick { get; set; }
        public float ColonyWealth { get; set; }
        public int ColonistCount { get; set; }
        public string Storyteller { get; set; }
        public bool IsPaused { get; set; }
    }

    public class ModInfoDto
    {
        public string Name { get; set; }
        public string PackageId { get; set; }
        public int LoadOrder { get; set; }
    }
}
