using RimworldRestApi.Core;
using Verse;

namespace RIMAPI
{
    public class RIMAPI_Settings : ModSettings
    {
        public string version = "0.5.4";
        public string apiVersion = "v1";
        public int serverPort = 8765;
        public int refreshIntervalTicks = 300;
        public bool _enableLogging = false;
        public int _loggingLevel = 0;

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
            DebugLogging.IsLogging = _enableLogging;
            DebugLogging.LoggingLevel = (LoggingLevels)_loggingLevel;
            Log.Message($"Logging setting changed. Enabled: {DebugLogging.IsLogging}, Level: {DebugLogging.LoggingLevel}");
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref _enableLogging, "enableLogging", false);
            Scribe_Values.Look(ref _loggingLevel, "loggingLevel", 1);
            Scribe_Values.Look(ref serverPort, "serverPort", 8765);
            Scribe_Values.Look(ref refreshIntervalTicks, "refreshIntervalTicks", 300);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                OnSettingChanged();
            }
        }
    }
}
