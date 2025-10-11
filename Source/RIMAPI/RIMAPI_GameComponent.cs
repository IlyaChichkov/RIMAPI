using System;
using RimworldRestApi.Core;
using RimworldRestApi.Services;
using Verse;

namespace RIMAPI
{
    public class RIMAPI_GameComponent : GameComponent
    {
        private int tickCounter;
        private ApiServer _apiServer;
        private bool _serverInitialized;
        private IGameDataService _gameDataService;

        public RIMAPI_GameComponent(Game game) : base()
        {
            // Initialize services
            _gameDataService = new GameDataService();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            try
            {
                // Initialize API server with injected dependencies
                _apiServer = new ApiServer(
                    RIMAPI_Mod.Settings.serverPort,
                    _gameDataService
                );
                _apiServer.Start();
                _serverInitialized = true;

                Log.Message($"RIMAPI: REST API Server started on port {RIMAPI_Mod.Settings.serverPort}");
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Failed to start API server - {ex.Message}");
                _serverInitialized = false;
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (!_serverInitialized) return;

            // Process queued HTTP requests every tick (even when paused)
            _apiServer.ProcessQueuedRequests();

            // Update game data service with current tick
            _gameDataService.UpdateGameTick(Find.TickManager.TicksGame);

            tickCounter++;
            if (tickCounter >= RIMAPI_Mod.Settings.refreshIntervalTicks)
            {
                tickCounter = 0;
                _apiServer.RefreshDataCache();
            }
        }

        public override void GameComponentOnGUI()
        {
            base.GameComponentOnGUI();

            if (!_serverInitialized) return;

            // Process additional requests during GUI for better responsiveness
            _apiServer.ProcessQueuedRequests();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            // Save server state if needed in future
        }
    }
}