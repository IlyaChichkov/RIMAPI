using System;
using RIMAPI.Core;
using UnityEngine;
using Verse;

namespace RIMAPI
{
    public class RIMAPI_GameComponent : GameComponent
    {
        private int tickCounter;
        private static ApiServer _apiServer;
        private static bool _serverInitialized;
        private static readonly object _serverLock = new object();

        public RIMAPI_GameComponent(Game game)
            : base() { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            GameObject go = new GameObject("MyTextureExporter");
            go.AddComponent<TextureExportManager>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        public static void StartServer()
        {
            lock (_serverLock)
            {
                if (_serverInitialized)
                {
                    LogApi.Info("Server already initialized, skipping...");
                    return;
                }

                try
                {
                    LogApi.IsLogging = RIMAPI_Mod.Settings.EnableLogging;

                    _apiServer?.Dispose();

                    // Create ApiServer with DI provider
                    _apiServer = new ApiServer(RIMAPI_Mod.Settings);
                    RIMAPI_Mod.RegisterSseService(_apiServer.SseService);
                    _apiServer.Start();
                    _serverInitialized = true;

                    LogApi.Info(
                        $"REST API Server started on port {RIMAPI_Mod.Settings.serverPort}"
                    );

                    var extensions = _apiServer.GetExtensions();
                    if (extensions.Count > 0)
                    {
                        LogApi.Info($"API Server loaded {extensions.Count} extensions");
                    }
                }
                catch (Exception ex)
                {
                    LogApi.Error($"Failed to start API server - {ex.Message}");
                    _serverInitialized = false;
                }
            }
        }

        public static void RegisterExtension(IRimApiExtension extension)
        {
            if (_serverInitialized && _apiServer != null)
            {
                _apiServer.RegisterExtension(extension);
            }
            else
            {
                LogApi.Warning(
                    $"Cannot register extension {extension.ExtensionName} - server not initialized"
                );
            }
        }

        public static IRimApiExtension GetExtension(string extensionId)
        {
            return _serverInitialized ? _apiServer?.GetExtension(extensionId) : null;
        }

        public static void RestartServer()
        {
            Log.Message("Restarting API server...");
            Shutdown();
            StartServer();
        }

        public static void ProcessServerQueues()
        {
            if (!_serverInitialized || _apiServer == null)
                return;

            _apiServer.ProcessQueuedRequests();
            _apiServer.ProcessBroadcastQueue();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (!_serverInitialized || _apiServer == null)
                return;

            // Process queued HTTP requests every tick
            _apiServer.ProcessQueuedRequests();

            // Process any queued SSE broadcasts
            _apiServer.ProcessBroadcastQueue();

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

            if (!_serverInitialized || _apiServer == null)
                return;

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
