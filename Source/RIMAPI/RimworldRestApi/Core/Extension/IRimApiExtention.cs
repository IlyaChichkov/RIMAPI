namespace RIMAPI.Core
{
    /// <summary>
    /// Interface for mods to extend RIMAPI with custom endpoints, services, and events
    /// </summary>
    public interface IRimApiExtension
    {
        /// <summary>
        /// Unique identifier for the extension (e.g., "hospitality", "combat-extended")
        /// </summary>
        string ExtensionId { get; }

        /// <summary>
        /// Human-readable name for the extension
        /// </summary>
        string ExtensionName { get; }

        /// <summary>
        /// Version of the extension
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Register extension-specific services with DI container
        /// </summary>
        /// <param name="services">Service collection for dependency injection</param>
        void RegisterServices(IServiceCollection services);

        /// <summary>
        /// Called during API server initialization to register custom endpoints
        /// </summary>
        /// <param name="router">Router for adding custom routes</param>
        void RegisterEndpoints(IExtensionRouter router);

        /// <summary>
        /// Register custom events for Server-Sent Events (SSE)
        /// </summary>
        /// <param name="eventRegistry">Event registry for SSE event types</param>
        void RegisterEvents(IEventRegistry eventRegistry);
    }
}
