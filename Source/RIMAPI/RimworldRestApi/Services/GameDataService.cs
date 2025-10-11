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
    }
}