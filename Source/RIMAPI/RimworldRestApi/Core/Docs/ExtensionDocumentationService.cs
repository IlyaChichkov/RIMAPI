using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Verse;

namespace RIMAPI.Core
{
    public class ExtensionDocumentationService
    {
        private readonly ExtensionRegistry _extensionRegistry;
        private readonly IDocumentationService _coreDocService;

        public ExtensionDocumentationService(
            ExtensionRegistry extensionRegistry,
            IDocumentationService coreDocService
        )
        {
            _extensionRegistry = extensionRegistry;
            _coreDocService = coreDocService;
        }

        public bool TryExportExtensionDocumentation(string extensionId, string format = "markdown")
        {
            try
            {
                var extension = _extensionRegistry.Extensions.FirstOrDefault(e =>
                    e.ExtensionId == extensionId
                );

                if (extension == null)
                {
                    LogApi.Error($"Extension '{extensionId}' not found for documentation export");
                    return false;
                }

                var modFolder = FindExtensionModFolder(extension);
                if (string.IsNullOrEmpty(modFolder))
                {
                    LogApi.Warning($"Could not find mod folder for extension '{extensionId}'");
                    return false;
                }

                string content;
                string fileName;

                switch (format.ToLower())
                {
                    case "json":
                        var docs = GenerateExtensionDocumentation(extension);
                        content = JsonConvert.SerializeObject(docs, Formatting.Indented);
                        fileName = $"{extensionId}_api.json";
                        break;

                    case "markdown":
                    default:
                        content = GenerateExtensionMarkdown(extension);
                        fileName = $"{extensionId}_api.md";
                        break;
                }

                var filePath = Path.Combine(modFolder, fileName);
                File.WriteAllText(filePath, content, Encoding.UTF8);

                LogApi.Info($"Exported {extensionId} documentation to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error exporting documentation for extension '{extensionId}': {ex}");
                return false;
            }
        }

        private string FindExtensionModFolder(IRimApiExtension extension)
        {
            try
            {
                var extensionType = extension.GetType();
                var extensionAssembly = extensionType.Assembly;

                // Look through all loaded mods to find which one contains this extension
                foreach (var mod in LoadedModManager.RunningMods)
                {
                    try
                    {
                        if (
                            mod.assemblies?.loadedAssemblies != null
                            && mod.assemblies.loadedAssemblies.Contains(extensionAssembly)
                        )
                        {
                            return mod.RootDir;
                        }
                    }
                    catch
                    {
                        // Skip mods that cause issues
                        continue;
                    }
                }

                // Fallback: try to find by extension ID
                var modsFolder = GenFilePaths.ModsFolderPath;
                var potentialFolder = Path.Combine(modsFolder, extension.ExtensionId);
                if (Directory.Exists(potentialFolder))
                {
                    return potentialFolder;
                }

                // Last resort: use extension ID as folder name in mods folder
                return Path.Combine(modsFolder, extension.ExtensionId);
            }
            catch (Exception ex)
            {
                LogApi.Error(
                    $"Error finding mod folder for extension '{extension.ExtensionId}': {ex}"
                );
                return null;
            }
        }

        private string GenerateExtensionMarkdown(IRimApiExtension extension)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# {extension.ExtensionName} API Documentation");
            sb.AppendLine();
            sb.AppendLine($"**Extension ID**: `{extension.ExtensionId}`  ");
            sb.AppendLine($"**Version**: {extension.Version}  ");
            sb.AppendLine($"**Generated**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC  ");
            sb.AppendLine();

            sb.AppendLine("## Description");
            sb.AppendLine(
                "This documentation is automatically generated from the extension's API endpoints."
            );
            sb.AppendLine();

            // Get endpoints for this extension from core documentation
            var allDocs = _coreDocService.GenerateDocumentation();
            var extensionSection = allDocs.Sections.FirstOrDefault(s =>
                s.Name.Equals(extension.ExtensionName, StringComparison.OrdinalIgnoreCase)
            );

            if (extensionSection != null && extensionSection.Endpoints.Any())
            {
                sb.AppendLine("## Endpoints");
                sb.AppendLine();

                foreach (var endpoint in extensionSection.Endpoints)
                {
                    sb.AppendLine($"### {endpoint.Method} {endpoint.Path}");
                    sb.AppendLine();
                    sb.AppendLine(endpoint.Description);
                    sb.AppendLine();

                    if (!string.IsNullOrEmpty(endpoint.ResponseExample))
                    {
                        sb.AppendLine("**Response Example:**");
                        sb.AppendLine("```json");
                        sb.AppendLine(endpoint.ResponseExample);
                        sb.AppendLine("```");
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                sb.AppendLine("> No endpoints documented for this extension.");
                sb.AppendLine("> The extension may not have registered any endpoints yet.");
                sb.AppendLine();
            }

            sb.AppendLine("## Usage");
            sb.AppendLine("```bash");
            sb.AppendLine($"# Base URL: http://localhost:8080/api/v1/{extension.ExtensionId}");
            sb.AppendLine($"curl http://localhost:8080/api/v1/{extension.ExtensionId}/endpoint");
            sb.AppendLine("```");

            return sb.ToString();
        }

        private object GenerateExtensionDocumentation(IRimApiExtension extension)
        {
            var allDocs = _coreDocService.GenerateDocumentation();
            var extensionSection = allDocs.Sections.FirstOrDefault(s =>
                s.Name.Equals(extension.ExtensionName, StringComparison.OrdinalIgnoreCase)
            );

            return new
            {
                extension = new
                {
                    id = extension.ExtensionId,
                    name = extension.ExtensionName,
                    version = extension.Version,
                },
                generatedAt = DateTime.UtcNow,
                endpoints = extensionSection?.Endpoints ?? new List<DocumentedEndpoint>(),
            };
        }
    }
}
