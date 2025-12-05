using System.Collections.Generic;
using RIMAPI.CameraStreamer;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    // Core Infrastructure
    public interface IGameDataService
    {
        void RefreshCache();
        void UpdateGameTick(int currentTick);
    }

    public interface IGameStateService
    {
        ApiResult<GameStateDto> GetGameState();
        ApiResult<List<ModInfoDto>> GetModsInfo();
        ApiResult DeselectAll();
        ApiResult OpenTab(string tabName);
        ApiResult<DefsDto> GetAllDefs();
        ApiResult<MapTimeDto> GetCurrentMapDatetime();
        ApiResult<MapTimeDto> GetWorldTileDatetime(int tileID);
    }

    // Colonist Service
    public interface IColonistService
    {
        ApiResult<List<ColonistDto>> GetColonists();
        ApiResult<ColonistDto> GetColonist(int pawnId);
        ApiResult<List<ColonistDetailedDto>> GetColonistsDetailed();
        ApiResult<ColonistDetailedDto> GetColonistDetailed(int pawnId);
        ApiResult<ColonistInventoryDto> GetColonistInventory(int pawnId);
        ApiResult<BodyPartsDto> GetColonistBodyParts(int pawnId);
        ApiResult<OpinionAboutPawnDto> GetOpinionAboutPawn(int pawnId, int otherPawnId);
        ApiResult<WorkListDto> GetWorkList();
        ApiResult<List<TimeAssignmentDto>> GetTimeAssignmentsList();
        ApiResult SetColonistWorkPriority(int pawnId, string workDef, int priority);
        ApiResult<TraitDefDto> GetTraitDefDto(string traitName);
        ApiResult SetTimeAssignment(int pawnId, int hour, string assignmentName);

        ApiResult<List<OutfitDto>> GetOutfits();
        ApiResult EditPawn(PawnEditRequest request);
        ApiResult<ImageDto> GetPawnPortraitImage(
            int pawnId,
            int width,
            int height,
            string direction
        );
    }

    // Map Service
    public interface IMapService
    {
        ApiResult<List<MapDto>> GetMaps();
        ApiResult<MapPowerInfoDto> GetMapPowerInfo(int mapId);
        ApiResult<MapWeatherDto> GetWeather(int mapId);
        ApiResult<List<AnimalDto>> GetMapAnimals(int mapId);
        ApiResult<List<ThingDto>> GetMapThings(int mapId);
        ApiResult<MapCreaturesSummaryDto> GetMapCreaturesSummary(int mapId);
        ApiResult<MapFarmSummaryDto> GenerateFarmSummary(int mapId);
        ApiResult<GrowingZoneDto> GetGrowingZoneById(int mapId, int zoneId);
        ApiResult<MapZonesDto> GetMapZones(int mapId);
        ApiResult<MapRoomsDto> GetMapRooms(int mapId);
        ApiResult<List<BuildingDto>> GetMapBuildings(int mapId);
    }

    // Building Service
    public interface IBuildingService
    {
        ApiResult<BuildingDto> GetBuildingInfo(int buildingId);
    }

    // Research Service
    public interface IResearchService
    {
        ApiResult<ResearchProjectDto> GetResearchProgress();
        ApiResult<ResearchFinishedDto> GetResearchFinished();
        ApiResult<ResearchTreeDto> GetResearchTree();
        ApiResult<ResearchProjectDto> GetResearchProjectByName(string name);
        ApiResult<ResearchSummaryDto> GetResearchSummary();
    }

    // Incident & Quest Service
    public interface IIncidentService
    {
        ApiResult<QuestsDto> GetQuestsData(int mapId);
        ApiResult<IncidentsDto> GetIncidentsData(int mapId);
        ApiResult<List<LordDto>> GetLordsData(int mapId);
        ApiResult TriggerIncident(TriggerIncidentRequest request);
    }

    // Resource Service
    public interface IResourceService
    {
        ApiResult<ResourcesSummaryDto> GetResourcesSummary(int mapId);
        ApiResult<StoragesSummaryDto> GetStoragesSummary(int mapId);
        ApiResult<Dictionary<string, List<ThingDto>>> GetAllStoredResources(int mapId);
        ApiResult<List<ThingDto>> GetAllStoredResourcesByCategory(int mapId, string categoryDef);
    }

    // Job Service
    public interface IJobService { }

    // Image Service
    public interface IImageService
    {
        ApiResult<ImageDto> GetItemImage(string name);
        ApiResult SetItemImageByName(ImageUploadRequest request);
        ApiResult SetStuffColor(StuffColorRequest request);
    }

    // Faction Service
    public interface IFactionService
    {
        ApiResult<List<FactionsDto>> GetFactions();
        ApiResult<FactionDto> GetFaction(int id);
        ApiResult<FactionDto> GetPlayerFaction();
        ApiResult<FactionRelationDto> GetFactionRelationWith(int id, int otherId);
        ApiResult<FactionRelationsDto> GetFactionRelations(int id);
        ApiResult<FactionDefDto> GetFactionDef(string defName);
        ApiResult<FactionChangeRelationResponceDto> ChangeFactionRelationWith(
            int id,
            int otherId,
            int change,
            bool sendMessage,
            bool canSendHostilityLetter
        );
    }

    // Dev Tools Service
    public interface IDevToolsService
    {
        ApiResult<MaterialsAtlasList> GetMaterialsAtlasList();
        ApiResult MaterialsAtlasPoolClear();
        ApiResult ConsoleAction(string action, string message = null);
        ApiResult SetStuffColor(StuffColorRequest stuffColor);
    }

    // Camera Service
    public interface ICameraService
    {
        ApiResult ChangeZoom(int zoom);
        ApiResult MoveToPosition(int x, int y);
        ApiResult StartStream(ICameraStream stream);
        ApiResult StopStream(ICameraStream stream);
        ApiResult SetupStream(ICameraStream stream, StreamConfigDto config);
        ApiResult<StreamStatusDto> GetStreamStatus(ICameraStream stream);
    }
}
