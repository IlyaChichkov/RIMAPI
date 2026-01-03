using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimworldRestApi.Hooks
{
    /// <summary>
    /// Hooks global game state changes (Load, Save, Exit, New Game, Settings)
    /// and forwards them as SSE events.
    /// </summary>
    public static class GameStateHooks
    {
        private static void SendStateEvent(string eventName, object data = null)
        {
            try
            {
                var payload = new
                {
                    event_type = eventName,
                    ticks = Find.TickManager?.TicksGame ?? 0,
                    timestamp = DateTime.UtcNow,
                    data = data
                };

                EventPublisherAccess.Publish("game_state", payload);
            }
            catch (Exception ex)
            {
                var payload = new
                {
                    event_type = eventName,
                    data = ex.Message
                };

                EventPublisherAccess.Publish("error", payload);
            }
        }

        // --- 1. GAME LOADED ---
        [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
        public static class GameLoadPatch
        {
            static void Postfix()
            {
                SendStateEvent("game_loaded", new
                {
                    map_count = Find.Maps?.Count ?? 0,
                    permadeath = Current.Game.Info.permadeathMode,
                    storyteller = Find.Storyteller?.def?.defName
                });
            }
        }

        // --- 2. NEW GAME CREATED ---
        [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
        public static class GameNewPatch
        {
            static void Postfix()
            {
                SendStateEvent("new_game_created", new
                {
                    scenario = Find.Scenario?.name,
                    storyteller = Find.Storyteller?.def?.defName,
                    difficulty = Find.Storyteller?.difficultyDef?.defName
                });
            }
        }

        // --- 3. EXIT TO MENU ---
        [HarmonyPatch(typeof(GenScene), nameof(GenScene.GoToMainMenu))]
        public static class ExitToMenuPatch
        {
            static void Prefix()
            {
                // Prefix is better here because once GoToMainMenu finishes, the game state is cleared.
                SendStateEvent("exit_to_menu", new
                {
                    reason = "User requested main menu"
                });
            }
        }

        // --- 4. GAME SAVED ---
        [HarmonyPatch(typeof(GameDataSaveLoader), nameof(GameDataSaveLoader.SaveGame))]
        public static class GameSavePatch
        {
            static void Postfix(string fileName)
            {
                SendStateEvent("game_saved", new
                {
                    filename = fileName,
                    filesize_est = "unknown" // File info not available in this context easily
                });
            }
        }

        // --- 5. SETTINGS CHANGED ---
        // Prefs.Save() is called whenever standard settings or Mod settings are written to disk.
        [HarmonyPatch(typeof(Prefs), nameof(Prefs.Save))]
        public static class SettingsChangedPatch
        {
            static void Postfix()
            {
                SendStateEvent("settings_changed", new
                {
                    volume_master = Prefs.VolumeMaster,
                    ui_scale = Prefs.UIScale
                });
            }
        }

        // --- 6. STORYTELLER / DIFFICULTY CHANGED ---
        // This hooks the closing of the Storyteller/Difficulty selection page in-game.
        // This is the most reliable way to detect runtime changes without spamming Tick checks.
        [HarmonyPatch(typeof(Page_SelectStorytellerInGame), "PreOpen")]
        public static class StorytellerOpenPatch
        {
            public static string OldStoryteller;
            public static string OldDifficulty;

            static void Postfix()
            {
                // Capture state before user edits
                OldStoryteller = Find.Storyteller?.def?.defName;
                OldDifficulty = Find.Storyteller?.difficultyDef?.defName;
            }
        }

        [HarmonyPatch(typeof(Page_SelectStorytellerInGame), "PostClose")]
        public static class StorytellerClosePatch
        {
            static void Postfix()
            {
                var newStoryteller = Find.Storyteller?.def?.defName;
                var newDifficulty = Find.Storyteller?.difficultyDef?.defName;

                // Check if anything actually changed
                if (newStoryteller != StorytellerOpenPatch.OldStoryteller ||
                    newDifficulty != StorytellerOpenPatch.OldDifficulty)
                {
                    SendStateEvent("storyteller_changed", new
                    {
                        old_storyteller = StorytellerOpenPatch.OldStoryteller,
                        new_storyteller = newStoryteller,
                        old_difficulty = StorytellerOpenPatch.OldDifficulty,
                        new_difficulty = newDifficulty
                    });
                }
            }
        }
    }
}