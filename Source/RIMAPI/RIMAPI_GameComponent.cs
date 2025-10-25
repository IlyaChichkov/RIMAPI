using System;
using RimworldRestApi.Core;
using RimworldRestApi.Services;
using Verse;

namespace RIMAPI
{
    public class RIMAPI_GameComponent : GameComponent
    {
        private int tickCounter;
        private static ApiServer _apiServer;
        private static bool _serverInitialized;
        private IGameDataService _gameDataService;
        private static readonly object _serverLock = new object();

        public RIMAPI_GameComponent(Game game) : base()
        {
            _gameDataService = new GameDataService();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            lock (_serverLock)
            {
                if (_serverInitialized)
                {
                    Log.Message("RIMAPI: Server already initialized, skipping...");
                    return;
                }

                try
                {
                    _apiServer?.Dispose();
                    _apiServer = new ApiServer(RIMAPI_Mod.Settings, _gameDataService);
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
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (!_serverInitialized || _apiServer == null) return;

            // Process queued HTTP requests every tick
            _apiServer.ProcessQueuedRequests();

            // Process any queued SSE broadcasts
            _apiServer.ProcessBroadcastQueue();

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

            if (!_serverInitialized || _apiServer == null) return;

            // Process additional requests during GUI for better responsiveness
            _apiServer.ProcessQueuedRequests();

            // Also process broadcasts during GUI
            _apiServer.ProcessBroadcastQueue();
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public static bool IsServerRunning()
        {
            return _serverInitialized && _apiServer != null;
        }

        public static void Shutdown()
        {
            lock (_serverLock)
            {
                if (_serverInitialized)
                {
                    Log.Message("RIMAPI: Shutting down API server...");
                    _apiServer?.Dispose();
                    _apiServer = null;
                    _serverInitialized = false;
                }
            }
        }
    }
}