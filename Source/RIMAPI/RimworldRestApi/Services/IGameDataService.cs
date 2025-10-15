using System.Collections.Generic;
using RimworldRestApi.Models;

namespace RimworldRestApi.Services
{
    public interface IGameDataService
    {
        // Game state
        GameStateDto GetGameState();
        List<ModInfoDto> GetModsInfo();

        // Colonists
        List<ColonistDto> GetColonists();
        ColonistDto GetColonist(int id);
        List<ColonistDetailedDto> GetColonistsDetailed();
        ColonistDetailedDto GetColonistDetailed(int id);
        ColonistInventoryDto GetColonistInventory(int id);
        ImageDto GetItemImage(string name);
        BodyPartsDto GetColonistBodyParts(int id);
        // Datetime
        MapTimeDto GetCurrentMapDatetime();
        MapTimeDto GetWorldTileDatetime(int tileID);

        // Map
        List<MapDto> GetMaps();
        MapPowerInfoDto GetMapPowerInfo(int mapId);
        List<AnimalDto> GetMapAnimals(int mapId);
        List<MapThingDto> GetMapThings(int mapId);
        MapCreaturesSummaryDto GetMapCreaturesSummary(int mapId);

        // Factions
        List<FactionsDto> GetFactions();

        // Resources
        ResourcesSummaryDto GetResourcesSummary(int mapId);
        StoragesSummaryDto GetStoragesSummary(int mapId);


        // Cache management
        void RefreshCache();
        void UpdateGameTick(int currentTick);
    }
}