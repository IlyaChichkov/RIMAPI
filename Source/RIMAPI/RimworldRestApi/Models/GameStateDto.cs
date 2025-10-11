using System;

namespace RimworldRestApi.Models
{
    public class GameStateDto
    {
        public int GameTick { get; set; }
        public float ColonyWealth { get; set; }
        public int ColonistCount { get; set; }
        public string Storyteller { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}