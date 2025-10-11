using System.Collections.Generic;
using RimworldRestApi.Models;

namespace RimworldRestApi.Services
{
    public interface IGameDataService
    {
        // Game state
        GameStateDto GetGameState();

        // Colonists
        List<ColonistDto> GetColonists();
        ColonistDto GetColonist(int id);

        // Cache management
        void RefreshCache();
        void UpdateGameTick(int currentTick);
    }
}