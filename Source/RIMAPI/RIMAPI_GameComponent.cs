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

            StartServer();
        }

        public void StartServer()
        {
            lock (_serverLock)
            {
                if (_serverInitialized)
                {
                    DebugLogging.Info("Server already initialized, skipping...");
                    return;
                }

                try
                {
                    DebugLogging.IsLogging = RIMAPI_Mod.Settings.EnableLogging;

                    _apiServer?.Dispose();
                    _apiServer = new ApiServer(RIMAPI_Mod.Settings, _gameDataService);
                    RIMAPI_Mod.RegisterSseService(_apiServer.SseService);
                    _apiServer.Start();
                    _serverInitialized = true;

                    DebugLogging.Info($"REST API Server started on port {RIMAPI_Mod.Settings.serverPort}");
                }
                catch (Exception ex)
                {
                    DebugLogging.Error($"Failed to start API server - {ex.Message}");
                    _serverInitialized = false;
                }
            }
        }

        public void RestartServer()
        {
            Log.Message("Restarting API server...");
            Shutdown();
            StartServer();
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
                    Log.Message("Shutting down API server...");
                    _apiServer?.Dispose();
                    _apiServer = null;
                    _serverInitialized = false;
                }
            }
        }
    }
}