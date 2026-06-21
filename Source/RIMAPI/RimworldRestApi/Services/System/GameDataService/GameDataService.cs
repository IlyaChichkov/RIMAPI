using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using Verse;

namespace RIMAPI.Services
{
    public class GameDataService : IGameDataService
    {
        private readonly ICachingService _cachingService;

        public GameDataService(ICachingService cachingService)
        {
            _cachingService = cachingService;
        }

        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = new System.Text.StringBuilder();
            result.Append(char.ToLower(input[0]));
            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    result.Append('_');
                    result.Append(char.ToLower(input[i]));
                }
                else
                {
                    result.Append(input[i]);
                }
            }
            return result.ToString();
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
                    ["StorytellerDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetStorytellerDefDtoList,
                            warnings,
                            "StorytellerDefs"
                        ),
                    ["DifficultyDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetDifficultyDefDtoList,
                            warnings,
                            "DifficultyDefs"
                        ),
                    ["JobDefs"] = () =>
                        SetProperty(
                            defs,
                            DefDatabaseHelper.GetJobDefDtoList,
                            warnings,
                            "JobDefs"
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
                        // Match either exact name or snake_case name
                        var matchedKey = propertyMap.Keys.FirstOrDefault(k =>
                            k.Equals(filter, StringComparison.OrdinalIgnoreCase) ||
                            ToSnakeCase(k).Equals(filter, StringComparison.OrdinalIgnoreCase));

                        if (matchedKey != null)
                        {
                            propertyMap[matchedKey]();
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
    }
}
