using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using RimWorld;
using RimworldRestApi.Core;
using RimworldRestApi.Helpers;
using RimworldRestApi.Models;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimworldRestApi.Services
{
    public class GameDataService : IGameDataService
    {
        private MapHelper _mapHelper;
        private FarmHelper _farmHelper;
        private GameEventsHelper _gameEventsHelper;
        private ResearchHelper _researchHelper;
        private ResourcesHelper _resourcesHelper;
        private ColonistsHelper _colonistsHelper;
        private BuildingHelper _buildingHelper;
        private TextureHelper _textureHelper;
        private int _lastCacheTick;
        private int needRefreshCooldownTicks = 60; // Refresh every 60 ticks
        private GameStateDto _cachedGameState;
        private List<MapDto> _cachedMaps;
        private List<ColonistDto> _cachedColonists;
        private List<ColonistDetailedDto> _cachedDetailedColonists;

        public GameDataService()
        {
            _mapHelper = new MapHelper();
            _farmHelper = new FarmHelper();
            _researchHelper = new ResearchHelper();
            _colonistsHelper = new ColonistsHelper();
            _textureHelper = new TextureHelper();
            _resourcesHelper = new ResourcesHelper();
            _gameEventsHelper = new GameEventsHelper();
            _buildingHelper = new BuildingHelper();
        }

        public void RefreshCache()
        {
            try
            {
                var game = Current.Game;

                // Update game state
                _cachedGameState = new GameStateDto
                {
                    GameTick = Find.TickManager?.TicksGame ?? 0,
                    ColonyWealth = GetColonyWealth(),
                    ColonistCount = GetColonistCount(),
                    Storyteller = game?.storyteller?.def?.defName ?? "Unknown",
                    LastUpdate = DateTime.UtcNow
                };

                // Update colonists cache
                _cachedColonists = _colonistsHelper.GetColonists();
                _cachedDetailedColonists = _colonistsHelper.GetColonistsDetailed();

                // Update maps cache
                _cachedMaps = _mapHelper.GetMaps();

                _lastCacheTick = Find.TickManager?.TicksGame ?? 0;
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Error refreshing cache - {ex.Message}");
            }
        }

        public void UpdateGameTick(int currentTick)
        {
            // Track game time for cache invalidation
        }

        private bool NeedsRefresh()
        {
            return (Find.TickManager?.TicksGame ?? 0) - _lastCacheTick > needRefreshCooldownTicks;
        }

        public GameStateDto GetGameState()
        {
            if (_cachedGameState == null || NeedsRefresh())
            {
                RefreshCache();
            }
            return _cachedGameState;
        }

        public List<MapDto> GetMaps()
        {
            if (_cachedMaps == null || NeedsRefresh())
            {
                RefreshCache();
            }
            return _cachedMaps;
        }

        public MapPowerInfoDto GetMapPowerInfo(int mapId)
        {
            return _mapHelper.GetMapPowerInfoInternal(mapId);
        }

        public List<ColonistDto> GetColonists()
        {
            if (_cachedColonists == null || NeedsRefresh())
            {
                RefreshCache();
            }
            return _cachedColonists;
        }

        public ColonistDto GetColonist(int id)
        {
            return GetColonists().FirstOrDefault(c => c.Id == id);
        }

        public List<ColonistDetailedDto> GetColonistsDetailed()
        {
            if (_cachedDetailedColonists == null || NeedsRefresh())
            {
                RefreshCache();
            }
            return _cachedDetailedColonists;
        }

        public ColonistDetailedDto GetColonistDetailed(int id)
        {
            return GetColonistsDetailed().FirstOrDefault(c => c.Colonist.Id == id);
        }

        private float GetColonyWealth()
        {
            try
            {
                return Find.CurrentMap?.wealthWatcher?.WealthTotal ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private int GetColonistCount()
        {
            try
            {
                return Find.CurrentMap?.mapPawns?.FreeColonistsCount ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public ColonistInventoryDto GetColonistInventory(int id)
        {
            Pawn colonist = PawnsFinder.AllMaps_FreeColonists.Where(
                p => p.thingIDNumber == id
            ).FirstOrDefault();

            try
            {
                List<ThingDto> Items = new List<ThingDto>();
                List<ThingDto> Apparels = new List<ThingDto>();
                List<ThingDto> Equipment = new List<ThingDto>();

                foreach (var item in colonist.inventory.innerContainer)
                {
                    Items.Add(ResourcesHelper.ThingToDto(item));
                }

                foreach (var apparel in colonist.apparel.WornApparel)
                {
                    Items.Add(ResourcesHelper.ThingToDto(apparel));
                }

                foreach (var equipment in colonist.equipment.AllEquipmentListForReading)
                {
                    Items.Add(ResourcesHelper.ThingToDto(equipment));
                }

                return new ColonistInventoryDto
                {
                    Items = Items,
                    Apparels = Apparels,
                    Equipment = Equipment,
                };
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Error getting colonist inventory - {ex.Message}");
            }

            return new ColonistInventoryDto();
        }

        public BodyPartsDto GetColonistBodyParts(int id)
        {
            BodyPartsDto bodyParts = new BodyPartsDto();

            try
            {
                Pawn colonist = PawnsFinder.AllMaps_FreeColonists.Where(
                    p => p.thingIDNumber == id
                ).FirstOrDefault();

                Material bodyMaterial = colonist.Drawer.renderer.BodyGraphic.MatAt(Rot4.South);
                Texture2D bodyTexture = (Texture2D)bodyMaterial.mainTexture;

                Material headMaterial = colonist.Drawer.renderer.HeadGraphic.MatAt(Rot4.South);
                Texture2D headTexture = (Texture2D)headMaterial.mainTexture;

                bodyParts.BodyImage = _textureHelper.TextureToBase64(bodyTexture);
                bodyParts.BodyColor = bodyMaterial.color.ToString();
                bodyParts.HeadImage = _textureHelper.TextureToBase64(headTexture);
                bodyParts.HeadColor = headMaterial.color.ToString();
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Error getting body image - {ex.Message}");
            }
            return bodyParts;
        }

        public List<ModInfoDto> GetModsInfo()
        {
            List<ModInfoDto> modsInfo = new List<ModInfoDto>();

            try
            {
                foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
                {
                    modsInfo.Add(new ModInfoDto
                    {
                        Name = mod.Name,
                        PackageId = mod.PackageId,
                        LoadOrder = mod.loadOrder,
                    });
                }
                return modsInfo;
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Error getting mods list - {ex.Message}");
                return modsInfo;
            }
        }

        public ImageDto GetItemImage(string name)
        {
            return _textureHelper.GetItemImageByName(name);
        }

        public ImageDto GetPawnPortraitImage(int pawnId, int width, int height, string direction)
        {
            Pawn pawn = _colonistsHelper.GetPawnById(pawnId);
            return _textureHelper.GetPawnPortraitImage(pawn, width, height, direction);
        }

        public MapTimeDto GetCurrentMapDatetime()
        {
            return _mapHelper.GetDatetimeAt(MapHelper.GetMapTileId(Find.CurrentMap));
        }

        public MapTimeDto GetWorldTileDatetime(int tileID)
        {
            return _mapHelper.GetDatetimeAt(tileID);
        }

        public List<FactionsDto> GetFactions()
        {
            List<FactionsDto> factions = new List<FactionsDto>();
            try
            {
                if (Current.ProgramState != ProgramState.Playing || Find.FactionManager == null)
                {
                    return factions;
                }

                factions = Find.FactionManager.AllFactionsListForReading
                    .Select(f => new FactionsDto
                    {
                        Name = f.Name,
                        Def = f.def?.defName,
                        IsPlayer = f.IsPlayer,
                        Relation = f.IsPlayer ? string.Empty :
                            (
                                Find.FactionManager?.OfPlayer != null
                                ? Find.FactionManager.OfPlayer.RelationKindWith(f).ToString()
                                : string.Empty
                            ),
                        Goodwill = f.IsPlayer ? 0 :
                            (Find.FactionManager?.OfPlayer?.GoodwillWith(f) ?? 0),
                    })
                    .ToList();

                return factions;
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Error getting factions list - {ex.Message}");
                return new List<FactionsDto>();
            }

        }

        public List<AnimalDto> GetMapAnimals(int mapId)
        {
            return _mapHelper.GetMapAnimals(mapId);
        }

        public List<ThingDto> GetMapThings(int mapId)
        {
            return _mapHelper.GetMapThings(mapId);
        }

        public ResourcesSummaryDto GetResourcesSummary(int mapId)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            return _resourcesHelper.GenerateResourcesSummary(map);
        }

        public StoragesSummaryDto GetStoragesSummary(int mapId)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            return _resourcesHelper.StoragesSummary(map);
        }

        public MapCreaturesSummaryDto GetMapCreaturesSummary(int mapId)
        {
            return _mapHelper.GetMapCreaturesSummary(mapId);
        }

        public MapFarmSummaryDto GenerateFarmSummary(int mapId)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            return _farmHelper.GenerateFarmSummary(map);
        }

        public GrowingZoneDto GetGrowingZoneById(int mapId, int zoneId)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            return _farmHelper.GetGrowingZoneById(map, zoneId);
        }

        public MapWeatherDto GetWeather(int mapId)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            return new MapWeatherDto
            {
                Weather = map.weatherManager?.curWeather?.defName,
                Temperature = map.mapTemperature?.OutdoorTemp ?? 0f,
            };
        }

        public ResearchProjectDto GetResearchProgress()
        {
            return _researchHelper.GetResearchProgress();
        }

        public ResearchFinishedDto GetResearchFinished()
        {
            return _researchHelper.GetResearchFinished();
        }

        public ResearchTreeDto GetResearchTree()
        {
            return _researchHelper.GetResearchTree();
        }

        public ResearchProjectDto GetResearchProjectByName(string name)
        {
            return _researchHelper.GetResearchProjectByName(name);
        }

        public ResearchSummaryDto GetResearchSummary()
        {
            return _researchHelper.GetResearchSummary();
        }

        public OpinionAboutPawnDto GetOpinionAboutPawn(int id, int otherId)
        {
            Pawn pawn = PawnsFinder.AllMaps_FreeColonists.Where(
                p => p.thingIDNumber == id
            ).FirstOrDefault();
            if (pawn == null) throw new ArgumentException("Failed to find pawn by id");

            Pawn other = PawnsFinder.AllMaps_FreeColonists.Where(
                p => p.thingIDNumber == otherId
            ).FirstOrDefault();
            if (other == null) throw new ArgumentException("Failed to find other pawn by id");

            return new OpinionAboutPawnDto
            {
                Opinion = pawn.relations.OpinionOf(other),
                OpinionAboutMe = other.relations.OpinionOf(pawn),
            };
        }

        public QuestsDto GetIncidentQuestData(int mapId)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            return _gameEventsHelper.GetQuestsDto(map);
        }

        public QuestsDto GetQuestsData(int mapId)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            return _gameEventsHelper.GetQuestsDto(map);
        }

        public IncidentsDto GetIncidentsData(int mapId)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            return new IncidentsDto
            {
                Incidents = _gameEventsHelper.GetIncidentsLog(map),
            };
        }

        public MapZonesDto GetMapZones(int mapId)
        {
            MapZonesDto mapZones = new MapZonesDto();
            mapZones.Zones = _mapHelper.GetMapZones(mapId);
            mapZones.Areas = _mapHelper.GetMapAreas(mapId);
            return mapZones;
        }

        public List<BuildingDto> GetMapBuildings(int mapId)
        {
            return _mapHelper.GetMapBuildings(mapId);
        }

        public BuildingDto GetBuildingInfo(int buildingId)
        {
            Building building = _buildingHelper.FindBuildingByID(buildingId);
            if (building == null)
            {
                throw new Exception("Building with this id wasn't found");
            }

            // Turret Info
            if (building is Building_Turret)
            {
                return _buildingHelper.GetTurretInfo(building);
            }

            // Generator Info
            if (building.TryGetComp<CompPowerPlant>() != null)
            {
                return _buildingHelper.GetPowerGeneratorInfo(building);
            }

            return _buildingHelper.BuildingToDto(building);
        }

        public void SelectGameObject(string objectType, int id)
        {
            switch (objectType)
            {
                case "item":
                    var item = Find.CurrentMap.listerThings.AllThings.Where(p => p.thingIDNumber == id).FirstOrDefault();
                    Find.Selector.Select(item);
                    break;
                case "pawn":
                    var pawn = _colonistsHelper.GetPawnById(id);
                    Find.Selector.Select(pawn);
                    break;
                case "building":
                    var building = _buildingHelper.FindBuildingByID(id);
                    Find.Selector.Select(building);
                    break;
                default:
                    throw new Exception($"Tried to select unknown object type: {objectType}");
            }
        }

        public void OpenTab(string tabName)
        {
            switch (tabName.ToLower())
            {
                case "health":
                    InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Health));
                    break;
                case "character":
                case "backstory":
                    InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Character));
                    break;
                case "gear":
                case "equipment":
                case "inventory":
                    InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Gear));
                    break;
                case "needs":
                case "mood":
                    InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Needs));
                    break;
                case "training":
                    InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Training));
                    break;
                case "log":
                case "combatlog":
                    InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Log));
                    break;
                default:
                    throw new Exception($"Tried to open unknown tab menu: {tabName}");
            }
        }

        public void DeselectAll()
        {
            Find.Selector.ClearSelection();
        }

        public Dictionary<string, List<ThingDto>> GetAllStoredResources(int mapId)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            var storageLocations = _resourcesHelper.GetAllStorageLocations(map);
            return _resourcesHelper.GetStoredItemsByCategory(storageLocations);
        }

        public List<ThingDto> GetAllStoredResourcesByCategory(int mapId, string categoryDef)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            var storageLocations = _resourcesHelper.GetAllStorageLocations(map);
            DebugLogging.Info("categoryDef: " + categoryDef);
            return _resourcesHelper.GetStoredItemsListByCategory(storageLocations, categoryDef);
        }

        public void MakeJobEquip(int mapId, int pawnId, int equipmentId, string equipmentType)
        {
            Map map = _mapHelper.FindMapByUniqueID(mapId);
            if (map == null)
            {
                throw new Exception($"Map with ID={mapId} not found");
            }
            Pawn pawn = map.listerThings.AllThings
                .OfType<Pawn>()
                .FirstOrDefault(p => p.thingIDNumber == pawnId);
            if (pawn == null)
            {
                throw new Exception($"Pawn with ID={pawnId} not found");
            }

            Thing foundThing = map.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == equipmentId);
            if (foundThing == null)
            {
                throw new Exception($"Thing with ID={equipmentId} not found");
            }

            Job job = null;
            switch (equipmentType)
            {
                case "weapon":
                    if (EquipmentUtility.CanEquip(foundThing, pawn) == false)
                    {
                        throw new Exception($"Can't equip this weapon");
                    }

                    job = JobMaker.MakeJob(JobDefOf.Equip, foundThing);
                    break;
                case "apparel":
                    if (ApparelUtility.HasPartsToWear(pawn, foundThing.def) == false)
                    {
                        throw new Exception($"Can't equip this apparel");
                    }

                    job = JobMaker.MakeJob(JobDefOf.Wear, foundThing);
                    break;
            }

            if (job == null)
            {
                throw new Exception($"Failed to make a job");
            }

            bool result = pawn.jobs.TryTakeOrderedJob(job);
            if (!result)
            {
                throw new Exception($"Failed to assign job to pawn");
            }
        }

        public void SetColonistWorkPriority(int pawnId, string workDef, int priority)
        {
            // Find the pawn by thingIDNumber  
            Pawn pawn = _colonistsHelper.GetPawnById(pawnId);
            if (pawn == null)
            {
                throw new Exception($"Could not find pawn with ID {pawnId}");
            }

            // Find the WorkTypeDef by defName  
            WorkTypeDef workTypeDef = DefDatabase<WorkTypeDef>.GetNamedSilentFail(workDef);
            if (workTypeDef == null)
            {
                throw new Exception($"Could not find WorkTypeDef with defName {workDef}");
            }

            // Check if pawn has work settings initialized  
            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
            {
                throw new Exception($"Pawn {pawn.LabelShort} does not have work settings initialized");
            }

            // Check if the work type is disabled for this pawn  
            if (priority != 0 && pawn.WorkTypeIsDisabled(workTypeDef))
            {
                throw new Exception($"Cannot set priority for disabled work type {workTypeDef.defName} on pawn {pawn.LabelShort}");
            }

            // Validate priority range (0-9)  
            if (priority < 0 || priority > 9)
            {
                throw new Exception($"Invalid priority {priority}. Must be between 0 and 4");
            }

            // Set the priority
            pawn.workSettings.SetPriority(workTypeDef, priority);

            RefreshCache();
        }

        public void SetColonistsWorkPriorities(int pawnId, string workDef, int priority)
        {
            // Find the pawn by thingIDNumber  
            Pawn pawn = _colonistsHelper.GetPawnById(pawnId);
            if (pawn == null)
            {
                throw new Exception($"Could not find pawn with ID {pawnId}");
            }

            // Find the WorkTypeDef by defName  
            WorkTypeDef workTypeDef = null;
            foreach (WorkTypeDef workType in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (workType.defName.ToLower() == workDef.ToLower())
                {
                    workTypeDef = workType;
                }
            }
            if (workTypeDef == null)
            {
                throw new Exception($"Could not find WorkTypeDef with defName {workDef}");
            }

            // Check if pawn has work settings initialized  
            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
            {
                throw new Exception($"Pawn {pawn.LabelShort} does not have work settings initialized");
            }

            // Check if the work type is disabled for this pawn  
            if (priority != 0 && pawn.WorkTypeIsDisabled(workTypeDef))
            {
                throw new Exception($"Cannot set priority for disabled work type {workTypeDef.defName} on pawn {pawn.LabelShort}");
            }

            // Validate priority range (0-9)  
            if (priority < 0 || priority > 9)
            {
                throw new Exception($"Invalid priority {priority}. Must be between 0 and 4");
            }

            // Set the priority
            pawn.workSettings.SetPriority(workTypeDef, priority);

            RefreshCache();
        }

        public WorkListDto GetWorkList()
        {
            WorkListDto workList = new WorkListDto
            {
                Work = new List<string>()
            };

            foreach (WorkTypeDef workType in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (workType == null) continue;
                workList.Work.Add(workType.defName);
            }

            return workList;
        }
    }
}