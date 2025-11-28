using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace RIMAPI.Core
{
    /// <summary>
    /// Manages registration and discovery of RIMAPI extensions
    /// </summary>
    public class ExtensionRegistry
    {
        private readonly List<IRimApiExtension> _extensions;
        private readonly object _lock = new object();
        private bool _initialized = false;
        public IReadOnlyList<IRimApiExtension> Extensions => GetExtensions();
        public bool IsInitialized => _initialized;

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
                LogApi.Error("Attempted to register null extension");
                return;
            }

            lock (_lock)
            {
                if (_extensions.Any(e => e.ExtensionId == extension.ExtensionId))
                {
                    LogApi.Warning(
                        $"Extension with ID '{extension.ExtensionId}' already registered"
                    );
                    return;
                }

                _extensions.Add(extension);
                LogApi.Info(
                    $"Registered extension '{extension.ExtensionName}' ({extension.ExtensionId}) v{extension.Version}"
                );
            }
        }

        /// <summary>
        /// Register extension services with DI container
        /// </summary>
        public void RegisterExtensionServices(IServiceCollection services)
        {
            lock (_lock)
            {
                foreach (var extension in _extensions)
                {
                    try
                    {
                        extension.RegisterServices(services);
                        LogApi.Info(
                            $"Registered services for extension: {extension.ExtensionName}"
                        );
                    }
                    catch (Exception ex)
                    {
                        LogApi.Error(
                            $"Failed to register services for {extension.ExtensionName}: {ex.Message}"
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Register extension events with event registry
        /// </summary>
        public void RegisterExtensionEvents(IEventRegistry eventRegistry)
        {
            lock (_lock)
            {
                foreach (var extension in _extensions)
                {
                    try
                    {
                        extension.RegisterEvents(eventRegistry);
                        LogApi.Info($"Registered events for extension: {extension.ExtensionName}");
                    }
                    catch (Exception ex)
                    {
                        LogApi.Error(
                            $"Failed to register events for {extension.ExtensionName}: {ex.Message}"
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Automatically discover and register extensions via reflection
        /// </summary>
        public void DiscoverExtensions()
        {
            if (_initialized)
            {
                LogApi.Warning("Extension discovery already completed");
                return;
            }

            try
            {
                LogApi.Info("Scanning for extensions...");

                // Scan all loaded assemblies for IRimApiExtension implementations
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        ScanAssemblyForExtensions(assembly);
                    }
                    catch (Exception ex)
                    {
                        LogApi.Error(
                            $"Error scanning assembly {assembly.GetName().Name}: {ex.Message}"
                        );
                    }
                }

                _initialized = true;
                LogApi.Info($"Extension discovery complete. Found {_extensions.Count} extensions.");
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error during extension discovery: {ex}");
            }
        }

        private void ScanAssemblyForExtensions(Assembly assembly)
        {
            try
            {
                var extensionTypes = assembly
                    .GetTypes()
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
                        LogApi.Error(
                            $"Failed to create extension instance {type.Name}: {ex.Message}"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // Some assemblies may not be accessible, just log and continue
                LogApi.Info($"Could not scan assembly {assembly.GetName().Name}: {ex.Message}");
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

        /// <summary>
        /// Initialize all extensions with the provided router
        /// </summary>
        public void InitializeExtensionEndpoints(IExtensionRouter router)
        {
            lock (_lock)
            {
                foreach (var extension in _extensions)
                {
                    try
                    {
                        extension.RegisterEndpoints(router);
                        LogApi.Info(
                            $"Registered endpoints for extension: {extension.ExtensionName}"
                        );
                    }
                    catch (Exception ex)
                    {
                        LogApi.Error(
                            $"Failed to register endpoints for {extension.ExtensionName}: {ex.Message}"
                        );
                    }
                }
            }
        }
    }
}
