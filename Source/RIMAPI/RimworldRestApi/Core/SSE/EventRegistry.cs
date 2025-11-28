using System.Collections.Generic;
using System.Linq;

namespace RIMAPI.Core
{
    public class EventRegistry : IEventRegistry
    {
        private readonly SseService _sseService;
        private readonly HashSet<string> _registeredEvents;
        private readonly object _lock = new object();

        public EventRegistry(SseService sseService)
        {
            _sseService = sseService;
            _registeredEvents = new HashSet<string>();
        }

        public void RegisterEventType(string eventType)
        {
            lock (_lock)
            {
                _registeredEvents.Add(eventType);
            }

            // Also register with SSE service for validation
            _sseService.RegisterEventType(eventType);

            LogApi.Info($"Registered event type: {eventType}");
        }

        public void PublishEvent(string eventType, object data)
        {
            // TODO: registeration of SSE events
            // if (!IsEventTypeRegistered(eventType))
            // {
            //     Logger.Warning($"Attempted to publish unregistered event type: {eventType}");
            //     return;
            // }

            // Use the new BroadcastEvent method
            _sseService.BroadcastEvent(eventType, data);
        }

        public bool IsEventTypeRegistered(string eventType)
        {
            lock (_lock)
            {
                return _registeredEvents.Contains(eventType);
            }
        }

        public IReadOnlyList<string> GetRegisteredEvents()
        {
            lock (_lock)
            {
                return _registeredEvents.ToList();
            }
        }
    }
}
