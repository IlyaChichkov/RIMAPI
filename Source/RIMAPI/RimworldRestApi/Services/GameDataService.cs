using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Services
{
    public class GameStateService : IGameStateService
    {
        public GameStateService() { }

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
                // Implementation for opening specific tabs
                // This would depend on your specific tab system
                LogApi.Message($"Opening tab: {tabName}");
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error opening tab {tabName}: {ex}");
                return ApiResult.Fail($"Failed to open tab: {ex.Message}");
            }
        }

        public ApiResult<DefsDto> GetAllDefs()
        {
            try
            {
                var defs = new DefsDto();

                // Add implementations for different def types
                // You can move this to a dedicated DefService later

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

                var vector = Find.WorldGrid.LongLatOf(GetMapTileId(Find.CurrentMap));
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
    }
}
