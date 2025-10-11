using System;
using System.Collections.Generic;
using System.Linq;
using RimworldRestApi.Models;
using Verse;

namespace RimworldRestApi.Services
{
    public class GameDataService : IGameDataService
    {
        private int _lastCacheTick;
        private GameStateDto _cachedGameState;
        private List<ColonistDto> _cachedColonists;
        private List<ColonistDetailedDto> _cachedDetailedColonists;

        public GameStateDto GetGameState()
        {
            if (_cachedGameState == null || NeedsRefresh())
            {
                RefreshCache();
            }
            return _cachedGameState;
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
                _cachedColonists = GetColonistsInternal();
                _cachedDetailedColonists = GetColonistsDetailedInternal();

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
            return (Find.TickManager?.TicksGame ?? 0) - _lastCacheTick > 60; // Refresh every 60 ticks
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

        private List<ColonistDto> GetColonistsInternal()
        {
            var colonists = new List<ColonistDto>();

            try
            {
                var map = Find.CurrentMap;
                if (map == null) return colonists;

                var freeColonists = map.mapPawns?.FreeColonists;
                if (freeColonists == null) return colonists;

                foreach (var pawn in freeColonists)
                {
                    if (pawn == null) continue;

                    colonists.Add(new ColonistDto
                    {
                        Id = pawn.thingIDNumber,
                        Name = pawn.Name?.ToStringShort ?? "Unknown",
                        Gender = pawn.gender.ToString(),
                        Age = pawn.ageTracker.AgeBiologicalYears,
                        Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
                        Mood = pawn.needs?.mood?.CurLevelPercentage ?? 0.5f,
                        Position = new PositionDto
                        {
                            X = pawn.Position.x,
                            Y = pawn.Position.y,
                            Z = pawn.Position.z
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting colonists - {ex.Message}");
            }

            return colonists;
        }

        private List<ColonistDetailedDto> GetColonistsDetailedInternal()
        {
            var colonists = new List<ColonistDetailedDto>();

            try
            {
                var map = Find.CurrentMap;
                if (map == null) return colonists;

                var freeColonists = map.mapPawns?.FreeColonists;
                if (freeColonists == null) return colonists;

                foreach (var pawn in freeColonists)
                {
                    if (pawn == null) continue;

                    colonists.Add(PawnToDetailedDto(pawn));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting detailed colonists - {ex.Message}");
            }

            return colonists;
        }

        private ColonistDetailedDto PawnToDetailedDto(Pawn pawn)
        {
            try
            {
                return new ColonistDetailedDto
                {
                    Id = pawn.thingIDNumber,
                    Name = pawn.Name?.ToStringShort ?? "Unknown",
                    Age = pawn.ageTracker?.AgeBiologicalYears ?? 0,
                    Gender = pawn.gender.ToString(),
                    Position = new PositionDto
                    {
                        X = pawn.Position.x,
                        Y = pawn.Position.z
                    },
                    Mood = (pawn.needs?.mood?.CurLevelPercentage ?? -1f) * 100,
                    Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
                    Hediffs = GetHediffs(pawn),
                    CurrentJob = pawn.CurJob?.def?.defName ?? "",
                    Traits = GetTraits(pawn),
                    WorkPriorities = GetWorkPriorities(pawn)
                };
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error converting pawn to DTO - {ex.Message}");
                return new ColonistDetailedDto { Id = pawn.thingIDNumber, Name = "Error" };
            }
        }

        private List<HediffDto> GetHediffs(Pawn pawn)
        {
            try
            {
                return pawn.health?.hediffSet?.hediffs?
                    .Where(h => h != null)
                    .Select(h => new HediffDto
                    {
                        Part = h.Part?.Label,
                        Label = h.Label
                    })
                    .ToList() ?? new List<HediffDto>();
            }
            catch
            {
                return new List<HediffDto>();
            }
        }

        private List<string> GetTraits(Pawn pawn)
        {
            try
            {
                return pawn.story?.traits?.allTraits?
                    .Where(t => t != null)
                    .Select(t => t.def.defName)
                    .ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private List<WorkPriorityDto> GetWorkPriorities(Pawn pawn)
        {
            var priorities = new List<WorkPriorityDto>();

            try
            {
                if (pawn.workSettings == null) return priorities;

                foreach (var workType in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    if (workType == null) continue;

                    var priority = pawn.workSettings.GetPriority(workType);
                    if (priority > 0)
                    {
                        priorities.Add(new WorkPriorityDto
                        {
                            WorkType = workType.defName,
                            Priority = priority
                        });
                    }
                }

                return priorities.OrderBy(p => p.Priority).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting work priorities for pawn {pawn.thingIDNumber} - {ex.Message}");
                return priorities;
            }
        }

    }
}