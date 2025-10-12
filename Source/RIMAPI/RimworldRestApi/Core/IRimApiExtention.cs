using System;
using System.Threading.Tasks;
using System.Net;

namespace RimworldRestApi.Core
{
    /// <summary>
    /// Interface for mods to extend RIMAPI with custom endpoints
    /// </summary>
    public interface IRimApiExtension
    {
        /// <summary>
        /// Unique identifier for the extension (e.g., "JobsMod", "MagicSystem")
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
        /// Called during API server initialization to register custom endpoints
        /// </summary>
        /// <param name="router">Router for adding custom routes</param>
        void RegisterEndpoints(IExtensionRouter router);
    }
}