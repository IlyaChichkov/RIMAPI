using System.Collections.Generic;
using RimworldRestApi.Models;

namespace RimworldRestApi.Services
{
    public interface IGameDataService
    {
        #region Game state
        GameStateDto GetGameState();
        List<ModInfoDto> GetModsInfo();
        void SelectGameObject(string objectType, int id);
        void DeselectAll();
        void OpenTab(string tabName);

        #endregion
        #region Colonists
        List<ColonistDto> GetColonists();
        ColonistDto GetColonist(int id);
        List<ColonistDetailedDto> GetColonistsDetailed();
        ColonistDetailedDto GetColonistDetailed(int id);
        ColonistInventoryDto GetColonistInventory(int id);
        ImageDto GetItemImage(string name);
        BodyPartsDto GetColonistBodyParts(int id);
        OpinionAboutPawnDto GetOpinionAboutPawn(int id, int otherId);
        #endregion
        #region Datetime
        MapTimeDto GetCurrentMapDatetime();
        MapTimeDto GetWorldTileDatetime(int tileID);

        #endregion
        #region Quests
        QuestsDto GetQuestsData(int mapId);
        IncidentsDto GetIncidentsData(int mapId);

        #endregion
        #region Map
        List<MapDto> GetMaps();
        MapPowerInfoDto GetMapPowerInfo(int mapId);
        MapWeatherDto GetWeather(int mapId);
        List<AnimalDto> GetMapAnimals(int mapId);
        List<MapThingDto> GetMapThings(int mapId);
        MapCreaturesSummaryDto GetMapCreaturesSummary(int mapId);
        MapFarmSummaryDto GenerateFarmSummary(int mapId);
        GrowingZoneDto GetGrowingZoneById(int mapId, int zoneId);
        MapZonesDto GetMapZones(int mapId);
        List<BuildingDto> GetMapBuildings(int mapId);
        #endregion
        #region Buildings
        BuildingDto GetBuildingInfo(int buildingId);
        #endregion
        #region Research
        ResearchProjectDto GetResearchProgress();
        ResearchFinishedDto GetResearchFinished();
        ResearchTreeDto GetResearchTree();
        ResearchProjectDto GetResearchProjectByName(string name);
        ResearchSummaryDto GetResearchSummary();

        #endregion
        #region Factions
        List<FactionsDto> GetFactions();

        #endregion
        #region Resources
        ResourcesSummaryDto GetResourcesSummary(int mapId);
        StoragesSummaryDto GetStoragesSummary(int mapId);


        #endregion
        #region Cache management
        void RefreshCache();
        void UpdateGameTick(int currentTick);
        #endregion
    }
}