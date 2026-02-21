using RIMAPI.Core;
using Verse;

namespace RIMAPI
{
    /// <summary>
    /// Manages the persistent configuration settings for the RIMAPI mod.
    /// <para>Handles serialization (save/load) of settings and provides property wrappers
    /// to trigger dynamic updates when configuration values change.</para>
    /// </summary>
    public class RIMAPI_Settings : ModSettings
    {
        // --- Version Info ---

        /// <summary>
        /// Current version of the RIMAPI mod.
        /// <para>Not saved to XML; used for UI display.</para>
        /// </summary>
        public string version = "1.8.2";

        /// <summary>
        /// Current supported API protocol version.
        /// <para>Not saved to XML; used for UI display.</para>
        /// </summary>
        public string apiVersion = "v1";


        // --- Server Configuration ---

        /// <summary>
        /// The IP address the HTTP server binds to.
        /// <para>Default: 0.0.0.0 (listens on all available network interfaces)</para>
        /// </summary>
        public string serverIP = "localhost";

        /// <summary>
        /// The local port number the HTTP server listens on.
        /// <para>Default: 8765</para>
        /// </summary>
        public int serverPort = 8765;

        /// <summary>
        /// The interval (in game ticks) at which the server refreshes its data cache.
        /// <para>Default: 300 ticks (approx. 5 seconds at normal speed).</para>
        /// </summary>
        public int refreshIntervalTicks = 300;


        // --- Logging Configuration (Private Backing Fields) ---

        /// <summary>
        /// Backing field for EnableLogging property.
        /// </summary>
        private bool _enableLogging = false;

        /// <summary>
        /// Backing field for LoggingLevel property.
        /// </summary>
        private int _loggingLevel = 0;


        // --- Caching Configuration ---

        /// <summary>
        /// Globally enables or disables data caching to improve performance.
        /// <para>Default: false</para>
        /// </summary>
        public bool EnableCaching = false;

        /// <summary>
        /// If true, cache hit/miss statistics will be logged to the console.
        /// <para>Default: true</para>
        /// </summary>
        public bool CacheLogStatistics = false;

        /// <summary>
        /// The default duration (in seconds) that cached data remains valid before expiry.
        /// <para>Default: 10 seconds</para>
        /// </summary>
        public int CacheDefaultExpirationSeconds = 10;


        // --- Properties with Change Triggers ---

        /// <summary>
        /// Toggles detailed debug logging for the API.
        /// <para>Setting this property automatically updates the static LogApi configuration.</para>
        /// </summary>
        public bool EnableLogging
        {
            get => _enableLogging;
            set
            {
                if (_enableLogging != value)
                {
                    _enableLogging = value;
                    OnSettingChanged();
                }
            }
        }

        /// <summary>
        /// Sets the minimum severity level for logs (0=Debug, 1=Info, etc.).
        /// <para>Setting this property automatically updates the static LogApi configuration.</para>
        /// </summary>
        public int LoggingLevel
        {
            get => _loggingLevel;
            set
            {
                if (_loggingLevel != value)
                {
                    _loggingLevel = value;
                    OnSettingChanged();
                }
            }
        }

        /// <summary>
        /// Propagates setting changes to the static LogApi utility immediately.
        /// </summary>
        private void OnSettingChanged()
        {
            LogApi.IsLogging = _enableLogging;
            // Cast integer to enum safely
            LogApi.LoggingLevel = (LoggingLevels)_loggingLevel;

            Log.Message(
                $"[RIMAPI] Logging setting updated. Enabled: {LogApi.IsLogging}, Level: {LogApi.LoggingLevel}"
            );
        }

        /// <summary>
        /// Serializes and deserializes settings data to/from the XML mod config file.
        /// <para>Also ensures LogApi state is restored correctly upon loading.</para>
        /// </summary>
        public override void ExposeData()
        {
            // Logging Settings
            Scribe_Values.Look(ref _enableLogging, "enableLogging", false);
            Scribe_Values.Look(ref _loggingLevel, "loggingLevel", 1);

            // Server Settings
            Scribe_Values.Look(ref serverIP, "serverIP", "localhost");
            Scribe_Values.Look(ref serverPort, "serverPort", 8765);
            Scribe_Values.Look(ref refreshIntervalTicks, "refreshIntervalTicks", 300);

            // Caching Settings
            Scribe_Values.Look(ref EnableCaching, "enableCaching", true);
            Scribe_Values.Look(ref CacheLogStatistics, "cacheLogStatistics", true);
            Scribe_Values.Look(ref CacheDefaultExpirationSeconds, "cacheDefaultExpirationSeconds", 60);

            // Post-Load Initialization
            // Ensure the static logger is updated immediately after settings are loaded from disk.
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                OnSettingChanged();
            }
        }
    }
}