using Verse;

namespace RimworldRestApi.Core
{
    public enum LoggingLevels
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        CRITICAL,
    }

    [StaticConstructorOnStartup]
    public class DebugLogging
    {
        public static bool IsLogging = false;
        public static LoggingLevels LoggingLevel = LoggingLevels.INFO;

        public static void Info(string text)
        {
            Message(text, LoggingLevels.INFO);
        }

        public static void Info(object text)
        {
            Message(text, LoggingLevels.INFO);
        }

        public static void Error(object text)
        {
            Message(text, LoggingLevels.ERROR);
        }

        public static void Warning(object text)
        {
            Message(text, LoggingLevels.WARNING);
        }


        public static void Message(string text, LoggingLevels messageLevel = LoggingLevels.INFO)
        {
            if (!IsLogging) return;
            if ((int)messageLevel < (int)LoggingLevel) return;

            string message = "[RIMAPI | ";
            switch (messageLevel)
            {
                case LoggingLevels.DEBUG:
                    message += "DEBUG] " + text;
                    Log.Message(message);
                    break;
                case LoggingLevels.INFO:
                    message += "INFO] " + text;
                    Log.Message(message);
                    break;
                case LoggingLevels.WARNING:
                    message += "WARNING] " + text;
                    Log.Warning(message);
                    break;
                case LoggingLevels.ERROR:
                    message += "ERROR] " + text;
                    Log.Error(message);
                    break;
                case LoggingLevels.CRITICAL:
                    message += "CRITICAL] " + text;
                    Log.Error(message);
                    break;
            }
        }

        public static void Message(object text, LoggingLevels messageLevel = LoggingLevels.INFO)
        {
            if (!IsLogging) return;
            if ((int)messageLevel < (int)LoggingLevel) return;

            string message = "[RIMAPI] " + text.ToString();
            switch (messageLevel)
            {
                case LoggingLevels.DEBUG:
                case LoggingLevels.INFO:
                    Log.Message(message);
                    break;
                case LoggingLevels.WARNING:
                    Log.Warning(message);
                    break;
                case LoggingLevels.ERROR:
                case LoggingLevels.CRITICAL:
                    Log.Error(message);
                    break;
            }
        }
    }
}