using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using UnityEngine.Experimental.AI;
using Verse;

namespace RIMAPI.Services
{
    public class GameStateService : IGameStateService
    {
        private readonly ICachingService _cachingService;

        public GameStateService(ICachingService cachingService)
        {
            _cachingService = cachingService;
        }

        public ApiResult<GameStateDto> GetGameState()
        {
            try
            {
                var state = new GameStateDto
                {
                    GameTick = Find.TickManager?.TicksGame ?? 0,
                    ColonyWealth = 4,
                    ColonistCount = 3,
                    Storyteller = Current.Game?.storyteller?.def?.defName ?? "Unknown",
                    IsPaused = Find.TickManager.Paused,
                };

                return ApiResult<GameStateDto>.Ok(state);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error getting game state: {ex}");
                return ApiResult<GameStateDto>.Fail($"Failed to get game state: {ex.Message}");
            }
        }

        public ApiResult<List<ModInfoDto>> GetModsInfo()
        {
            try
            {
                var mods = LoadedModManager
                    .RunningModsListForReading.Select(mod => new ModInfoDto
                    {
                        Name = mod.Name,
                        PackageId = mod.PackageId,
                        LoadOrder = mod.loadOrder,
                    })
                    .ToList();

                return ApiResult<List<ModInfoDto>>.Ok(mods);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error getting mods info: {ex}");
                return ApiResult<List<ModInfoDto>>.Fail($"Failed to get mods info: {ex.Message}");
            }
        }

        public ApiResult DeselectAll()
        {
            try
            {
                Find.Selector.ClearSelection();
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error deselecting all: {ex}");
                return ApiResult.Fail($"Failed to deselect: {ex.Message}");
            }
        }

        public ApiResult OpenTab(string tabName)
        {
            try
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
                    case "relations":
                    case "social":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Social));
                        break;
                    case "prisoner":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Prisoner));
                        break;
                    case "slave":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Slave));
                        break;
                    case "guest":
                        InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Guest));
                        break;
                    default:
                        return ApiResult.Fail($"Tried to open unknown tab menu: {tabName}");
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error opening tab {tabName}: {ex}");
                return ApiResult.Fail($"Failed to open tab: {ex.Message}");
            }
        }

        private void SetProperty<T>(
    DefsDto defs,
    Func<List<T>> valueGetter,
    List<string> warnings,
    string propertyName
        )
        {
            try
            {
                // Get compiled property setter from cache (or create and cache it)
                var propertySetter = _cachingService.GetPropertySetter<DefsDto, List<T>>(
                    propertyName
                );

                // Get the value
                var value = valueGetter();

                // Set the property using the compiled setter
                propertySetter(defs, value);
            }
            catch (Exception ex)
            {
                warnings.Add($"Failed to load {propertyName}: {ex.Message}");
            }
        }

        public ApiResult<DefsDto> GetAllDefs(AllDefsRequestDto body)
        {
            try
            {
                var warnings = new List<string>();
                var defs = new DefsDto();

                // Check if we should show all defs
                bool showAll =
                    body == null
                    || body.Filters == null
                    || body.Filters.Count == 0
                    || body.Filters.Contains("All", StringComparer.OrdinalIgnoreCase);

                // Create a dictionary of property setters for dynamic invocation
                var propertyMap = new Dictionary<string, Action>
                {
                    ["ThingsDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetThingDefDtoList,
                            warnings,
                            "ThingsDefs"
                        ),
                    ["IncidentsDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetIncidentDefDtoList,
                            warnings,
                            "IncidentsDefs"
                        ),
                    ["ConditionsDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetConditionsDefDtoList,
                            warnings,
                            "ConditionsDefs"
                        ),
                    ["PawnKindDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetPawnKindDefDtoList,
                            warnings,
                            "PawnKindDefs"
                        ),
                    ["TraitDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetTraitDefDtoList,
                            warnings,
                            "TraitDefs"
                        ),
                    ["ResearchDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetResearchProjectDefDtoList,
                            warnings,
                            "ResearchDefs"
                        ),
                    ["HediffDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetHediffDefsList,
                            warnings,
                            "HediffDefs"
                        ),
                    ["SkillDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetSkillDefDtoList,
                            warnings,
                            "SkillDefs"
                        ),
                    ["WorkTypeDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetWorkTypeDefDtoList,
                            warnings,
                            "WorkTypeDefs"
                        ),
                    ["NeedDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetNeedDefDtoList,
                            warnings,
                            "NeedDefs"
                        ),
                    ["ThoughtDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetThoughtDefDtoList,
                            warnings,
                            "ThoughtDefs"
                        ),
                    ["StatDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetStatDefDtoList,
                            warnings,
                            "StatDefs"
                        ),
                    ["WorldObjectDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetWorldObjectDefDtoList,
                            warnings,
                            "WorldObjectDefs"
                        ),
                    ["BiomeDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetBiomeDefDtoList,
                            warnings,
                            "BiomeDefs"
                        ),
                    ["TerrainDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetTerrainDefDtoList,
                            warnings,
                            "TerrainDefs"
                        ),
                    ["RecipeDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetRecipeDefDtoList,
                            warnings,
                            "RecipeDefs"
                        ),
                    ["BodyDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetBodyDefDtoList,
                            warnings,
                            "BodyDefs"
                        ),
                    ["BodyPartDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetBodyPartDefDtoList,
                            warnings,
                            "BodyPartDefs"
                        ),
                    ["FactionDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetFactionDefDtoList,
                            warnings,
                            "FactionDefs"
                        ),
                    ["SoundDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetSoundDefDtoList,
                            warnings,
                            "SoundDefs"
                        ),
                    ["DesignationCategoryDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetDesignationCategoryDefDtoList,
                            warnings,
                            "DesignationCategoryDefs"
                        ),
                    ["JoyKindDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetJoyKindDefDtoList,
                            warnings,
                            "JoyKindDefs"
                        ),
                    ["MemeDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetMemeDefDtoList,
                            warnings,
                            "MemeDefs"
                        ),
                    ["PreceptDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetPreceptDefDtoList,
                            warnings,
                            "PreceptDefs"
                        ),
                    ["AbilityDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetAbilityDefDtoList,
                            warnings,
                            "AbilityDefs"
                        ),
                    ["GeneDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetGeneDefDtoList,
                            warnings,
                            "GeneDefs"
                        ),
                    ["WeatherDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetWeatherDefDtoList,
                            warnings,
                            "WeatherDefs"
                        ),
                    ["RoomRoleDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetRoomRoleDefDtoList,
                            warnings,
                            "RoomRoleDefs"
                        ),
                    ["RoomStatDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetRoomStatDefDtoList,
                            warnings,
                            "RoomStatDefs"
                        ),
                    ["MentalStateDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetMentalStateDefDtoList,
                            warnings,
                            "MentalStateDefs"
                        ),
                    ["DrugPolicyDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetDrugPolicyDefDtoList,
                            warnings,
                            "DrugPolicyDefs"
                        ),
                    ["PlantDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetPlantDefDtoList,
                            warnings,
                            "PlantDefs"
                        ),
                    ["AnimalDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetAnimalDefDtoList,
                            warnings,
                            "AnimalDefs"
                        ),
                };

                // Execute only the requested properties
                if (showAll)
                {
                    // Execute all property getters
                    foreach (var propertySetter in propertyMap.Values)
                    {
                        propertySetter();
                    }
                }
                else
                {
                    // Execute only filtered properties
                    foreach (var filter in body.Filters)
                    {
                        if (propertyMap.TryGetValue(filter, out var propertySetter))
                        {
                            propertySetter();
                        }
                        else
                        {
                            warnings.Add($"Unknown filter: {filter}");
                        }
                    }
                }

                if (warnings.Count > 0)
                {
                    return ApiResult<DefsDto>.Partial(defs, warnings);
                }
                return ApiResult<DefsDto>.Ok(defs);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error getting all defs: {ex}");
                return ApiResult<DefsDto>.Fail($"Failed to get defs: {ex.Message}");
            }
        }

        public static int GetMapTileId(Map map)
        {
#if RIMWORLD_1_5
            return map.Tile;
#elif RIMWORLD_1_6
            return map.Tile.tileId;
#endif
            throw new Exception("Failed to get GetMapTileId for this rimworld version.");
        }

        public ApiResult<MapTimeDto> GetCurrentMapDatetime()
        {
            try
            {
                var map = Find.CurrentMap;
                if (map == null)
                    return ApiResult<MapTimeDto>.Fail("No current map found");

                var time = GetDatetimeAt(GetMapTileId(Find.CurrentMap));
                return ApiResult<MapTimeDto>.Ok(time);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error getting current map datetime: {ex}");
                return ApiResult<MapTimeDto>.Fail($"Failed to get datetime: {ex.Message}");
            }
        }

        public ApiResult<MapTimeDto> GetWorldTileDatetime(int tileID)
        {
            try
            {
                var time = GetDatetimeAt(tileID);

                return ApiResult<MapTimeDto>.Ok(time);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error getting world tile datetime for tile {tileID}: {ex}");
                return ApiResult<MapTimeDto>.Fail($"Failed to get datetime: {ex.Message}");
            }
        }

        public MapTimeDto GetDatetimeAt(int tileID)
        {
            MapTimeDto mapTimeDto = new MapTimeDto();
            try
            {
                if (Current.ProgramState != ProgramState.Playing || Find.WorldGrid == null)
                {
                    return mapTimeDto;
                }

                var vector = Find.WorldGrid.LongLatOf(tileID);
                mapTimeDto.Datetime = GenDate.DateFullStringWithHourAt(
                    Find.TickManager.TicksAbs,
                    vector
                );

                return mapTimeDto;
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error - {ex.Message}");
                return mapTimeDto;
            }
        }

        public ApiResult Select(string objectType, int id)
        {
            try
            {
                switch (objectType)
                {
                    case "item":
                        var item = Find
                            .CurrentMap.listerThings.AllThings.Where(p => p.thingIDNumber == id)
                            .FirstOrDefault();
                        Find.Selector.Select(item);
                        break;
                    case "pawn":
                        var pawn = ColonistsHelper.GetPawnById(id);
                        Find.Selector.Select(pawn);
                        break;
                    case "building":
                        var building = BuildingHelper.FindBuildingByID(id);
                        Find.Selector.Select(building);
                        break;
                    default:
                        return ApiResult.Fail($"Tried to select unknown object type: {objectType}");
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult SendLetterSimple(SendLetterRequestDto body)
        {
            try
            {
                List<string> warnings = new List<string>();
                const int MAX_LABEL_SIZE = 48;
                const int MAX_MESSAGE_SIZE = 500;
                var label = ApiSecurityHelper.SanitizeLetterInput(body.Label);
                var message = ApiSecurityHelper.SanitizeLetterInput(body.Message);

                if (string.IsNullOrEmpty(message))
                {
                    return ApiResult.Fail("Message is empty after sanitization");
                }

                if (message.Length > MAX_MESSAGE_SIZE)
                {
                    message = message.Substring(0, MAX_MESSAGE_SIZE) + "...";
                    warnings.Add($"Message has been truncated to {MAX_MESSAGE_SIZE} characters");
                }

                if (label.Length > MAX_LABEL_SIZE)
                {
                    message = message.Substring(0, MAX_LABEL_SIZE) + "...";
                    warnings.Add($"Label has been truncated to {MAX_LABEL_SIZE} characters");
                }

                LetterDef letterDef = GameTypesHelper.StringToLetterDef(body.LetterDef);
                LookTargets target = null;
                Faction faction = null;
                Quest quest = null;
                // TODO: Support for hyperlinkThingDefs & debugInfo
                List<ThingDef> hyperlinkThingDefs = null;
                string debugInfo = null;

                if (!string.IsNullOrEmpty(body.MapId))
                {
                    target = MapHelper.GetThingOnMapById(
                        int.Parse(body.MapId),
                        int.Parse(body.LookTargetThingId)
                    );
                }

                if (!string.IsNullOrEmpty(body.FactionOrderId))
                {
                    faction = FactionHelper.GetFactionByOrderId(int.Parse(body.FactionOrderId));
                }

                if (!string.IsNullOrEmpty(body.QuestId))
                {
                    int id = int.Parse(body.QuestId);
                    quest = Find
                        .QuestManager.QuestsListForReading.Where(s => s.id == id)
                        .FirstOrDefault();
                }

                Find.LetterStack.ReceiveLetter(
                    label,
                    message,
                    letterDef,
                    (LookTargets)target,
                    faction,
                    quest,
                    hyperlinkThingDefs,
                    debugInfo,
                    body.DelayTicks,
                    body.PlaySound
                );

                if (warnings.Count > 0)
                {
                    return ApiResult.Partial(warnings);
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult SetGameSpeed(int speed)
        {
            try
            {
                Find.TickManager.CurTimeSpeed = (TimeSpeed)speed;
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error setting game speed: {ex}");
                return ApiResult.Fail($"Failed to set game speed: {ex.Message}");
            }
        }

        public ApiResult SelectArea(SelectAreaRequestDto body)
        {
            try
            {
                if (body.PositionA == null || body.PositionB == null)
                {
                    return ApiResult.Fail("PositionA and PositionB cannot be null.");
                }

                IntVec3 posA = new IntVec3(body.PositionA.X, body.PositionA.Y, body.PositionA.Z);
                IntVec3 posB = new IntVec3(body.PositionB.X, body.PositionB.Y, body.PositionB.Z);

                CellRect rect = CellRect.FromLimits(posA, posB);
                Find.Selector.Select(rect);

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error selecting area: {ex}");
                return ApiResult.Fail($"Failed to select area: {ex.Message}");
            }
        }
    }
}
