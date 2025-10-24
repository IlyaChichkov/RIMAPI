using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RimWorld;
using RimworldRestApi.Helpers;
using RimworldRestApi.Models;
using UnityEngine;
using Verse;

namespace RimworldRestApi.Services
{
    public class GameDataService : IGameDataService
    {
        private MapHelper _mapHelper;
        private FarmHelper _farmHelper;
        private ResourcesHelper _resourcesHelper;
        private ColonistsHelper _colonistsHelper;
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
            _colonistsHelper = new ColonistsHelper();
            _textureHelper = new TextureHelper();
            _resourcesHelper = new ResourcesHelper();
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
                Log.Error($"RIMAPI: Error refreshing cache - {ex.Message}");
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
            return GetColonistsDetailed().FirstOrDefault(c => c.Id == id);
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
                List<InventoryThingDto> Items = new List<InventoryThingDto>();
                List<InventoryThingDto> Apparels = new List<InventoryThingDto>();
                List<InventoryThingDto> Equipment = new List<InventoryThingDto>();

                foreach (var item in colonist.inventory.innerContainer)
                {
                    Items.Add(new InventoryThingDto
                    {
                        ID = item.thingIDNumber,
                        Name = item.def.defName,
                        StackCount = item.stackCount
                    });
                }

                foreach (var apparel in colonist.apparel.WornApparel)
                {
                    Apparels.Add(new InventoryThingDto
                    {
                        ID = apparel.thingIDNumber,
                        Name = apparel.def.defName,
                        StackCount = apparel.stackCount
                    });
                }

                foreach (var equipment in colonist.equipment.AllEquipmentListForReading)
                {
                    Equipment.Add(new InventoryThingDto
                    {
                        ID = equipment.thingIDNumber,
                        Name = equipment.def.defName,
                        StackCount = equipment.stackCount
                    });
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
                Log.Error($"RIMAPI: Error getting colonist inventory - {ex.Message}");
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
                Log.Error($"RIMAPI: Error getting body image - {ex.Message}");
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
                Log.Error($"RIMAPI: Error getting mods list - {ex.Message}");
                return modsInfo;
            }
        }

        public ImageDto GetItemImage(string name)
        {
            ImageDto image = new ImageDto();

            try
            {
                var thingDef = DefDatabase<ThingDef>.GetNamed(name);
                Texture2D icon = null;

                if (!thingDef.uiIconPath.NullOrEmpty())
                {
                    icon = thingDef.uiIcon;
                }
                else
                {
                    icon = (Texture2D)thingDef.DrawMatSingle.mainTexture;
                }

                if (icon == null)
                {
                    image.Result = $"No icon available for item - {name}";
                }
                else
                {
                    image.Result = "Success";
                    image.ImageBase64 = _textureHelper.TextureToBase64(icon);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting item image - {ex.Message}");
            }

            return image;
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
                Log.Error($"RIMAPI: Error getting factions list - {ex.Message}");
                return new List<FactionsDto>();
            }

        }

        public List<AnimalDto> GetMapAnimals(int mapId)
        {
            return _mapHelper.GetMapAnimals(mapId);
        }

        public List<MapThingDto> GetMapThings(int mapId)
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

        public ResearchProgressDto GetResearchProgress()
        {
            ResearchManager researchManager = Find.ResearchManager;
            ResearchProjectDef currentProj = researchManager?.GetProject();

            return new ResearchProgressDto
            {
                CurrentProject = currentProj?.defName ?? "None",
                Label = currentProj?.label ?? "None",
                Progress = currentProj != null ? researchManager.GetProgress(currentProj) : 0f
            };
        }

        public ResearchFinishedDto GetResearchFinished()
        {
            ResearchManager researchManager = Find.ResearchManager;
            var finishedProjects = new List<string>();

            if (researchManager != null)
            {
                // Get all finished research projects
                finishedProjects = DefDatabase<ResearchProjectDef>.AllDefs
                    .Where(proj => researchManager.GetProgress(proj) >= proj.CostApparent)
                    .Select(proj => proj.defName)
                    .ToList();
            }

            return new ResearchFinishedDto
            {
                FinishedProjects = finishedProjects
            };
        }

        public ResearchTreeDto GetResearchTree()
        {
            ResearchManager researchManager = Find.ResearchManager;
            var projects = new List<ResearchProjectDto>();

            foreach (var projectDef in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                int progress = Mathf.RoundToInt(researchManager?.GetProgress(projectDef) ?? 0);
                bool isFinished = progress >= projectDef.CostApparent;

                projects.Add(new ResearchProjectDto
                {
                    Name = projectDef.defName,
                    Label = projectDef.label,
                    Progress = progress,
                    ResearchPoints = (int)projectDef.CostApparent,
                    Description = projectDef.Description,
                    IsFinished = isFinished,
                    IsAvailable = projectDef.CanStartNow,
                    TechLevel = projectDef.techLevel.ToString()
                });
            }

            return new ResearchTreeDto
            {
                Projects = projects.OrderBy(p => p.TechLevel).ThenBy(p => p.Name).ToList()
            };
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
    }
}