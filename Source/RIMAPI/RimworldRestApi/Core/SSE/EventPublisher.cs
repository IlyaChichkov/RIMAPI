using System;
using RIMAPI.Core;

namespace RIMAPI.Core
{
    public interface IEventPublisher
    {
        void Publish(string eventType, object data);
    }

    public class EventPublisher : IEventPublisher
    {
        private readonly IEventRegistry _eventRegistry;

        public EventPublisher(IEventRegistry eventRegistry)
        {
            _eventRegistry = eventRegistry;
        }

        public void Publish(string eventType, object data)
        {
            try
            {
                _eventRegistry.PublishEvent(eventType, data);
            }
            catch (Exception ex)
            {
                LogApi.Error($"[SSE] Error publishing event {eventType}: {ex}");
            }
        }
    }
}
