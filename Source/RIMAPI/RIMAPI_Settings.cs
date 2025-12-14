using RIMAPI.Core;
using Verse;

namespace RIMAPI
{
    public class RIMAPI_Settings : ModSettings
    {
        public string version = "1.4.0";
        public string apiVersion = "v1";
        public int serverPort = 8765;
        public int refreshIntervalTicks = 300;
        public bool _enableLogging = false;
        public int _loggingLevel = 0;

        public bool EnableCaching = true;
        public bool CacheLogStatistics = true;
        public int CacheDefaultExpirationSeconds = 60;

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

        private void OnSettingChanged()
        {
            LogApi.IsLogging = _enableLogging;
            LogApi.LoggingLevel = (LoggingLevels)_loggingLevel;
            Log.Message(
                $"Logging setting changed. Enabled: {LogApi.IsLogging}, Level: {LogApi.LoggingLevel}"
            );
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref _enableLogging, "enableLogging", false);
            Scribe_Values.Look(ref _loggingLevel, "loggingLevel", 1);
            Scribe_Values.Look(ref serverPort, "serverPort", 8765);
            Scribe_Values.Look(ref refreshIntervalTicks, "refreshIntervalTicks", 300);
            Scribe_Values.Look(ref EnableCaching, "enableCaching", true);
            Scribe_Values.Look(ref CacheLogStatistics, "cacheLogStatistics", true);
            Scribe_Values.Look(
                ref CacheDefaultExpirationSeconds,
                "cacheDefaultExpirationSeconds",
                60
            );

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                OnSettingChanged();
            }
        }
    }
}
