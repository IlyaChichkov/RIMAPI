

using System;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using RimWorld.Planet;
using Verse;

namespace RIMAPI.Helpers
{
    public static class GamePlayHelper
    {
        public static void InitGameFromConfiguration(NewGameStartRequestDto request)
        {
            StorytellerDef storytellerDef = DefDatabase<StorytellerDef>.GetNamed(request.StorytellerName);
            DifficultyDef difficultyDef = DefDatabase<DifficultyDef>.GetNamed(request.DifficultyName);

            string seed = string.IsNullOrEmpty(request.WorldSeed) ? GenText.RandomSeedString() : request.WorldSeed.ToLower();

            Current.ProgramState = ProgramState.Entry;
#if RIMWORLD_1_5
            // Game.ClearCaches(); not implimented in v1.5
#elif RIMWORLD_1_6
            Game.ClearCaches();
#endif
            Current.Game = new Game();
            Current.Game.InitData = new GameInitData();
            Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
            Find.Scenario.PreConfigure();
            Current.Game.storyteller = new Storyteller(storytellerDef, difficultyDef);
            GameInitData initData = Find.GameInitData;

#if RIMWORLD_1_5
            // LandmarkDensity not implimented in v1.5
            Current.Game.World = WorldGenerator.GenerateWorld(Mathf.Clamp(request.PlanetCoverage, 0, 1), seed, (OverallRainfall)request.OverallRainfall, (OverallTemperature)request.OverallTemperature, (OverallPopulation)request.OverallPopulation);
#elif RIMWORLD_1_6
            Current.Game.World = WorldGenerator.GenerateWorld(Mathf.Clamp(request.PlanetCoverage, 0, 1), seed, (OverallRainfall)request.OverallRainfall, (OverallTemperature)request.OverallTemperature, (OverallPopulation)request.OverallPopulation, (LandmarkDensity)request.LandmarkDensity);
#endif
            if (!string.IsNullOrEmpty(request.StartingTile) && request.StartingTile != "auto")
            {
                if (int.TryParse(request.StartingTile, out int tileId))
                {
                    if (tileId < 0 || tileId >= Find.WorldGrid.TilesCount)
                    {
                        Log.Error($"[RIMAPI] Requested tile {tileId} is out of bounds.");
                        throw new Exception("Tile ID value is out of boundaries");
                    }
#if RIMWORLD_1_5
                    initData.startingTile = tileId;
#elif RIMWORLD_1_6
                    initData.startingTile = Find.WorldGrid[tileId].tile;
#endif
                }
                else
                {
                    Log.Error($"[RIMAPI] Could not parse tile ID '{request.StartingTile}'.");
                    throw new Exception("Could not parse tile ID");
                }
            }
            else
            {
                initData.ChooseRandomStartingTile();
            }

            if (!string.IsNullOrEmpty(request.StartingTile) && request.StartingTile != "auto")
            {
                initData.startingSeason = (Season)int.Parse(request.StartingSeason);
            }
            else
            {
                initData.startingSeason = Season.Spring;
            }

            initData.mapSize = request.MapSize;
            initData.permadeath = request.Permadeath;
            Find.Scenario.PostIdeoChosen();
        }

        public static GameStateDto GetGameStateDto()
        {
            var state = new GameStateDto();

            try
            {
                // First check if we can access basic game state
                if (Current.Game == null || Find.Maps == null)
                {
                    // Game is not fully initialized
                    state.ProgramState = "NotInitialized";
                    state.GameTick = 0;
                    state.Storyteller = "Unknown";
                    state.MapCount = 0;
                    state.CurrentView = "None";
                    state.IsPaused = true;
                    return state;
                }

                state.GameTick = Find.TickManager?.TicksGame ?? 0;
                state.Storyteller = Current.Game.storyteller?.def?.defName ?? "Unknown";
                state.MapCount = Find.Maps.Count;
                state.ProgramState = Current.ProgramState.ToString();
            }
            catch (Exception)
            {
                // If any of the above fail, we're in an unstable state
                state.ProgramState = "Error";
                state.GameTick = 0;
                state.Storyteller = "Unknown";
                state.MapCount = 0;
                state.CurrentView = "Error";
                state.IsPaused = true;
                // Consider logging the exception if you have a logger
                // Log.Error($"Error getting game state: {ex.Message}");
                return state;
            }

            try
            {
                state.ColonyWealth = Find.CurrentMap?.wealthWatcher?.WealthTotal ?? 0;
            }
            catch
            {
                state.ColonyWealth = 0;
            }

            try
            {
                state.ColonistCount = Find.CurrentMap?.mapPawns?.FreeColonistsCount ?? 0;
            }
            catch
            {
                state.ColonistCount = 0;
            }

            // Set view state
            if (Current.ProgramState == ProgramState.Playing)
            {
                try
                {
#if RIMWORLD_1_5
            if (WorldRendererUtility.WorldRenderedNow)
            {
                state.CurrentView = "World";
            }
            else if (Find.CurrentMap != null)
            {
                state.CurrentView = "Map";
            }
            else
            {
                state.CurrentView = "Unknown";
            }
#elif RIMWORLD_1_6
            if (WorldRendererUtility.WorldRendered)
            {
                state.CurrentView = "World";
            }
            else if (Find.CurrentMap != null)
            {
                state.CurrentView = "Map";
            }
            else
            {
                state.CurrentView = "Unknown";
            }
#endif
                    state.IsPaused = Find.TickManager?.Paused ?? true;
                }
                catch
                {
                    state.CurrentView = "Error";
                    state.IsPaused = true;
                }
            }
            else
            {
                state.CurrentView = "Menu";
                state.IsPaused = true;
            }

            // Check for open windows
            if (Find.WindowStack != null)
            {
                try
                {
                    // Check for standard Options/Pause menu
                    state.IsSettingsOpen = Find.WindowStack.IsOpen(typeof(Dialog_Options));

                    // Check for Mod Settings
                    state.IsModSettingsOpen = Find.WindowStack.IsOpen(typeof(Dialog_ModSettings));
                }
                catch
                {
                    state.IsSettingsOpen = false;
                    state.IsModSettingsOpen = false;
                }
            }
            else
            {
                state.IsSettingsOpen = false;
                state.IsModSettingsOpen = false;
            }

            return state;
        }

        public static GameSettingsDto GetCurrentSettings()
        {
            return new GameSettingsDto
            {
                // --- General ---
                Language = Prefs.LangFolderName,
                RunInBackground = Prefs.RunInBackground,
                DevelopmentMode = Prefs.DevMode,
                LogVerbose = Prefs.LogVerbose,
                TemperatureMode = Prefs.TemperatureMode.ToString(),
                // Autosave is technically stored as a float of "days"
                AutosaveInterval = Prefs.AutosaveIntervalDays > 0,

                // --- Graphics ---
                Resolution = $"{Screen.width}x{Screen.height}",
                Fullscreen = Screen.fullScreen,
                UserInterfaceScale = Prefs.UIScale,
                CustomCursorEnabled = Prefs.CustomCursorEnabled,
                HatsOnlyOnMap = Prefs.HatsOnlyOnMap,
                PlantWindSway = Prefs.PlantWindSway,
                MaxNumberOfPlayerSettlements = Prefs.MaxNumberOfPlayerSettlements,

                // --- Audio ---
                VolumeMaster = Prefs.VolumeMaster,
                VolumeGame = Prefs.VolumeGame,
                VolumeMusic = Prefs.VolumeMusic,
                VolumeAmbient = Prefs.VolumeAmbient,

                // --- Gameplay ---
                PauseOnLoad = Prefs.PauseOnLoad,
                PauseOnUrgentLetter = Prefs.AutomaticPauseMode.ToString(),
                EdgeScreenScroll = Prefs.EdgeScreenScroll,
                MapDragSensitivity = Prefs.MapDragSensitivity,
                ZoomToMouse = Prefs.ZoomToMouse,

                // --- Interface ---
                ShowRealtimeClock = Prefs.ShowRealtimeClock,
                ResourceReadoutCategorized = Prefs.ResourceReadoutCategorized,
                ShowAnimalNames = Prefs.AnimalNameMode != AnimalNameDisplayMode.None,
            };
        }
    }

}