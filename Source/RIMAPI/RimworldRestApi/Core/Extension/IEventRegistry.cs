namespace RIMAPI.Core
{
    /// <summary>
    /// Registry for Server-Sent Events (SSE) from extensions
    /// </summary>
    public interface IEventRegistry
    {
        /// <summary>
        /// Register a custom event type for SSE
        /// </summary>
        /// <param name="eventType">Event type name (e.g., "hospitality:guest_arrived")</param>
        void RegisterEventType(string eventType);

        /// <summary>
        /// Publish an event to all connected SSE clients
        /// </summary>
        /// <param name="eventType">Event type name</param>
        /// <param name="data">Event data (will be serialized to JSON)</param>
        void PublishEvent(string eventType, object data);

        /// <summary>
        /// Check if an event type is registered
        /// </summary>
        bool IsEventTypeRegistered(string eventType);
    }
}
