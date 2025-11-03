using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace RimworldRestApi.Core
{
    /// <summary>
    /// Manages registration and discovery of RIMAPI extensions
    /// </summary>
    public class ExtensionRegistry
    {
        private readonly List<IRimApiExtension> _extensions;
        private readonly object _lock = new object();
        private bool _initialized = false;

        public ExtensionRegistry()
        {
            _extensions = new List<IRimApiExtension>();
        }

        /// <summary>
        /// Manually register an extension
        /// </summary>
        public void RegisterExtension(IRimApiExtension extension)
        {
            if (extension == null)
            {
                DebugLogging.Error("Attempted to register null extension");
                return;
            }

            lock (_lock)
            {
                if (_extensions.Any(e => e.ExtensionId == extension.ExtensionId))
                {
                    DebugLogging.Warning($"Extension with ID '{extension.ExtensionId}' already registered");
                    return;
                }

                _extensions.Add(extension);
                DebugLogging.Info($"Registered extension '{extension.ExtensionName}' ({extension.ExtensionId}) v{extension.Version}");
            }
        }

        /// <summary>
        /// Automatically discover and register extensions via reflection
        /// </summary>
        public void DiscoverExtensions()
        {
            if (_initialized)
            {
                DebugLogging.Warning("Extension discovery already completed");
                return;
            }

            try
            {
                DebugLogging.Info("Scanning for extensions...");

                // Scan all loaded assemblies for IRimApiExtension implementations
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        ScanAssemblyForExtensions(assembly);
                    }
                    catch (Exception ex)
                    {
                        DebugLogging.Error($"Error scanning assembly {assembly.GetName().Name}: {ex.Message}");
                    }
                }

                _initialized = true;
                DebugLogging.Info($"Extension discovery complete. Found {_extensions.Count} extensions.");
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Error during extension discovery: {ex}");
            }
        }

        private void ScanAssemblyForExtensions(Assembly assembly)
        {
            try
            {
                var extensionTypes = assembly.GetTypes()
                    .Where(t => typeof(IRimApiExtension).IsAssignableFrom(t))
                    .Where(t => !t.IsInterface && !t.IsAbstract);

                foreach (var type in extensionTypes)
                {
                    try
                    {
                        var extension = Activator.CreateInstance(type) as IRimApiExtension;
                        if (extension != null)
                        {
                            RegisterExtension(extension);
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLogging.Error($"Failed to create extension instance {type.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Some assemblies may not be accessible, just log and continue
                DebugLogging.Info($"Could not scan assembly {assembly.GetName().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all registered extensions
        /// </summary>
        public IReadOnlyList<IRimApiExtension> GetExtensions()
        {
            lock (_lock)
            {
                return new List<IRimApiExtension>(_extensions);
            }
        }

        /// <summary>
        /// Check if an extension is registered
        /// </summary>
        public bool HasExtension(string extensionId)
        {
            lock (_lock)
            {
                return _extensions.Any(e => e.ExtensionId == extensionId);
            }
        }
    }
}