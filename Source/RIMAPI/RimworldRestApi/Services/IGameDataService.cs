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
        List<ColonistDetailedDto> GetColonistsDetailed();
        ColonistDetailedDto GetColonistDetailed(int id);
        ColonistInventoryDto GetColonistInventory(int id);
        ImageDto GetItemImage(string name);
        BodyPartsDto GetColonistBodyParts(int id);
        MapTimeDto GetCurrentMapDatetime();
        MapTimeDto GetWorldTileDatetime(int tileID);

        // Map
        List<MapDto> GetMaps();
        MapPowerInfoDto GetMapPowerInfo(int mapId);

        // Cache management
        void RefreshCache();
        void UpdateGameTick(int currentTick);
    }
}