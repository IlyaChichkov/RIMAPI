using System;
using RIMAPI.Core;

public static class EventPublisherAccess
{
    private static IEventPublisher _publisher;
    private static readonly object _lock = new object();
    private static bool _initialized = false;

    public static void Initialize(IEventPublisher publisher)
    {
        lock (_lock)
        {
            _publisher = publisher;
            _initialized = true;
            LogApi.Info("EventPublisher initialized for patches");
        }
    }

    public static void Publish(string eventType, object data)
    {
        if (!_initialized || _publisher == null)
        {
            // Silently fail - patches might run before DI is set up
            return;
        }

        try
        {
            _publisher.Publish(eventType, data);
        }
        catch (Exception ex)
        {
            LogApi.Error($"[SSE] Error in EventPublisherAccess.Publish: {ex}");
        }
    }

    public static bool IsInitialized => _initialized;
}
