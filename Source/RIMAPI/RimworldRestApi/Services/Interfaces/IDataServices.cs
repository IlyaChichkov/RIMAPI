using System.Collections.Generic;
using RIMAPI.CameraStreamer;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    #region Core Infrastructure
    public interface IGameDataService
    {
        void RefreshCache();
        void UpdateGameTick(int currentTick);
    }

    public interface IGameStateService
    {
        ApiResult<GameStateDto> GetGameState();
        ApiResult<List<ModInfoDto>> GetModsInfo();
        ApiResult SelectArea(SelectAreaRequestDto body);
        ApiResult Select(string objectType, int id);
        ApiResult DeselectAll();
        ApiResult OpenTab(string tabName);
        ApiResult<DefsDto> GetAllDefs(AllDefsRequestDto body);
        ApiResult<MapTimeDto> GetCurrentMapDatetime();
        ApiResult<MapTimeDto> GetWorldTileDatetime(int tileID);
        ApiResult SendLetterSimple(SendLetterRequestDto body);
        ApiResult SetGameSpeed(int speed);
        ApiResult GameSave(string name);
        ApiResult GameLoad(string name);
        ApiResult GameDevQuickStart();
        ApiResult GameStart(NewGameStartRequestDto body);
        ApiResult<GameSettingsDto> GetCurrentSettings();
        ApiResult<bool> ToggleRunInBackground();
        ApiResult<bool> GetRunInBackground();
    }
    #endregion

    #region Colonists
    public interface IColonistService
    {
        ApiResult<List<PawnDto>> GetColonists();
        ApiResult<List<PawnPositionDto>> GetColonistPositions();
        ApiResult<PawnDto> GetColonist(int pawnId);
        ApiResult<List<ApiV1PawnDetailedDto>> GetColonistsDetailedV1();
        ApiResult<ApiV1PawnDetailedDto> GetColonistDetailedV1(int pawnId);
        ApiResult<List<PawnDetailedRequestDto>> GetColonistsDetailed();
        ApiResult<PawnDetailedRequestDto> GetColonistDetailed(int pawnId);
        ApiResult<PawnInventoryDto> GetColonistInventory(int pawnId);
        ApiResult<BodyPartsDto> GetColonistBodyParts(int pawnId);
        ApiResult<OpinionAboutPawnDto> GetOpinionAboutPawn(int pawnId, int otherPawnId);
        ApiResult<WorkListDto> GetWorkList();
        ApiResult<List<TimeAssignmentDto>> GetTimeAssignmentsList();
        ApiResult SetColonistWorkPriority(WorkPriorityRequestDto body);
        ApiResult SetColonistsWorkPriority(ColonistsWorkPrioritiesRequestDto body);
        ApiResult<TraitDefDto> GetTraitDefDto(string traitName);
        ApiResult SetTimeAssignment(PawnTimeAssignmentRequestDto body);
        ApiResult MakeJobEquip(int mapId, int pawnId, int equipmentId);

        ApiResult<List<OutfitDto>> GetOutfits();
        ApiResult<ImageDto> GetPawnPortraitImage(
            int pawnId,
            int width,
            int height,
            string direction
        );
    }
    #endregion

    #region Map Service
    public interface IMapService
    {
        ApiResult<List<MapDto>> GetMaps();
        ApiResult<MapPowerInfoDto> GetMapPowerInfo(int mapId);
        ApiResult<MapWeatherDto> GetWeather(int mapId);
        ApiResult<List<AnimalDto>> GetMapAnimals(int mapId);
        ApiResult<List<ThingDto>> GetMapThings(int mapId);
        ApiResult<List<ThingDto>> GetMapPlants(int mapId);
        ApiResult<MapCreaturesSummaryDto> GetMapCreaturesSummary(int mapId);
        ApiResult<MapFarmSummaryDto> GenerateFarmSummary(int mapId);
        ApiResult<GrowingZoneDto> GetGrowingZoneById(int mapId, int zoneId);
        ApiResult<MapZonesDto> GetMapZones(int mapId);
        ApiResult<MapRoomsDto> GetMapRooms(int mapId);
        ApiResult<List<BuildingDto>> GetMapBuildings(int mapId);
        ApiResult<MapTerrainDto> GetMapTerrain(int mapId);
        ApiResult<List<ThingDto>> GetMapThingsInRadius(int mapId, int x, int z, int radius);
        ApiResult SetWeather(int mapId, string defName);
        ApiResult<List<ThingDto>> GetThingsAtCell(ThingsAtCellRequestDto body);
        ApiResult DestroyCorpses(int mapId);
        ApiResult DestroyForbiddenItems(int mapId);
        ApiResult DestroyThingsInRect(DestroyRectRequestDto request);
        ApiResult RepairThingsAtPositions(RepairPositionsRequestDto request);
        ApiResult RepairThingsInRect(RepairRectRequestDto request);
        ApiResult SpawnDropPod(SpawnDropPodRequestDto request);
        ApiResult<FogGridDto> GetFogGrid(int mapId);
    }
    #endregion

    #region Building Service
    public interface IBuildingService
    {
        ApiResult<BuildingDto> GetBuildingInfo(int buildingId);
    }
    #endregion

    #region Research Service
    public interface IResearchService
    {
        ApiResult<ResearchProjectDto> GetResearchProgress();
        ApiResult<ResearchFinishedDto> GetResearchFinished();
        ApiResult<ResearchTreeDto> GetResearchTree();
        ApiResult<ResearchProjectDto> GetResearchProjectByName(string name);
        ApiResult<ResearchSummaryDto> GetResearchSummary();
    }
    #endregion

    #region Incident & Quest Service
    public interface IIncidentService
    {
        ApiResult<QuestsDto> GetQuestsData(int mapId);
        ApiResult<IncidentsDto> GetIncidentsData(int mapId);
        ApiResult<List<LordDto>> GetLordsData(int mapId);
        ApiResult TriggerIncident(TriggerIncidentRequestDto request);
    }
    #endregion

    #region Resource Service
    public interface IResourceService
    {
        ApiResult<ResourcesSummaryDto> GetResourcesSummary(int mapId);
        ApiResult<StoragesSummaryDto> GetStoragesSummary(int mapId);
        ApiResult<Dictionary<string, List<ThingDto>>> GetAllStoredResources(int mapId);
        ApiResult<List<ThingDto>> GetAllStoredResourcesByCategory(int mapId, string categoryDef);
        ApiResult SpawnItem(SpawnItemRequestDto body);
    }
    #endregion

    #region Job Service
    public interface IJobService { }
    #endregion

    #region Image Service
    public interface IImageService
    {
        ApiResult<ImageDto> GetItemImage(string name);
        ApiResult<ImageDto> GetTerrainImage(string name);
        ApiResult SetItemImageByName(ImageUploadRequest request);
        ApiResult SetStuffColor(StuffColorRequest request);
    }
    #endregion

    #region Dev Tools Service
    public interface IDevToolsService
    {
        ApiResult<MaterialsAtlasList> GetMaterialsAtlasList();
        ApiResult MaterialsAtlasPoolClear();
        ApiResult ConsoleAction(DebugConsoleRequest body);
        ApiResult SetStuffColor(StuffColorRequest stuffColor);
    }
    #endregion

    #region Camera Service
    public interface ICameraService
    {
        ApiResult ChangeZoom(int zoom);
        ApiResult MoveToPosition(int x, int y);
        ApiResult StartStream(ICameraStream stream);
        ApiResult StopStream(ICameraStream stream);
        ApiResult SetupStream(ICameraStream stream, StreamConfigDto config);
        ApiResult<StreamStatusDto> GetStreamStatus(ICameraStream stream);
    }
    #endregion
}
