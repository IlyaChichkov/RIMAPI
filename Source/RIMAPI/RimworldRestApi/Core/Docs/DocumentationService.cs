using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RIMAPI;
using RIMAPI.Controllers;
using RIMAPI.Models;

namespace RIMAPI.Core
{
    public interface IDocumentationService
    {
        ApiDocumentation GenerateDocumentation();
        string GenerateMarkdown();
    }

    public class DocumentationService : IDocumentationService
    {
        private readonly ExtensionRegistry _extensionRegistry;
        private readonly RIMAPI_Settings _settings;

        public DocumentationService(ExtensionRegistry extensionRegistry, RIMAPI_Settings settings)
        {
            _extensionRegistry = extensionRegistry;
            _settings = settings;
        }

        public ApiDocumentation GenerateDocumentation()
        {
            var docs = new ApiDocumentation
            {
                GeneratedAt = DateTime.UtcNow,
                BaseUrl = $"http://localhost:{_settings.serverPort}/api/v1",
                Version = _settings.version,
            };

            // Document core endpoints with enhanced information
            docs.Sections.Add(
                new DocumentationSection
                {
                    Name = "Core API",
                    Description = "Built-in RimWorld REST API endpoints",
                    Endpoints = DocumentCoreEndpointsWithExamples(),
                }
            );

            // Document extension endpoints
            foreach (var extension in _extensionRegistry.Extensions)
            {
                docs.Sections.Add(
                    new DocumentationSection
                    {
                        Name = extension.ExtensionName,
                        Description = $"Extension: {extension.ExtensionName} v{extension.Version}",
                        Endpoints = DocumentExtensionEndpointsWithExamples(extension.ExtensionId),
                    }
                );
            }

            return docs;
        }

        private List<DocumentedEndpoint> DocumentAllEndpointsAutomatically()
        {
            var endpoints = new List<DocumentedEndpoint>();

            // Auto-discover ALL controllers in the assembly
            var controllerTypes = DiscoverAllControllers();

            foreach (var controllerType in controllerTypes)
            {
                endpoints.AddRange(DocumentControllerWithExamples(controllerType));
            }

            return endpoints;
        }

        private List<Type> DiscoverAllControllers()
        {
            var assembly = typeof(ApiServer).Assembly;
            return assembly
                .GetTypes()
                .Where(t => t.Name.EndsWith("Controller") && !t.IsAbstract && !t.IsInterface)
                .ToList();
        }

        private List<DocumentedEndpoint> DocumentCoreEndpointsWithExamples()
        {
            var endpoints = DocumentAllEndpointsAutomatically();

            return endpoints;
        }

        private DocumentedEndpoint CreateDocumentedEndpoint(
            MethodInfo method,
            RouteAttribute routeAttr
        )
        {
            var descriptionAttr = method.GetCustomAttribute<EndpointDescriptionAttribute>();
            var responseExampleAttr = method.GetCustomAttribute<ResponseExampleAttribute>();

            var className = method.DeclaringType?.Name ?? "General";

            var declaringType = method.DeclaringType.Name;
            var methodName = method.Name;

            string githubLink =
                $"https://github.com/search?q=repo%3AIlyaChichkov%2FRIMAPI+path%3A%2F%28%5E%7C%5C%2F%29{declaringType}%5C.cs%24%2F+{methodName}&type=code";

            var toolTip =
                $"[{method.DeclaringType.Name}.{method.Name}]({githubLink} \"{methodName}\")";

            string description = "";
            description += string.IsNullOrEmpty(descriptionAttr?.Description)
                ? ""
                : descriptionAttr?.Description + "</br>";
            description += toolTip;

            var endpoint = new DocumentedEndpoint
            {
                Method = routeAttr.Method,
                Path = routeAttr.Pattern,
                Description = description,
                Category = descriptionAttr?.Category ?? className,
                Notes = descriptionAttr?.Notes,
                Parameters = ExtractEnhancedParameters(method),
                RequestExample = GenerateRequestExample(method),
                ResponseExample = GenerateEnhancedResponseExample(method, responseExampleAttr),
            };

            return endpoint;
        }

        private List<EndpointParameter> ExtractEnhancedParameters(MethodInfo method)
        {
            var parameters = new List<EndpointParameter>();

            foreach (var param in method.GetParameters())
            {
                if (param.ParameterType == typeof(HttpListenerContext))
                    continue;

                var paramDesc = param.GetCustomAttribute<ParameterDescriptionAttribute>();

                parameters.Add(
                    new EndpointParameter
                    {
                        Name = param.Name,
                        Type = GetTypeDisplayName(param.ParameterType),
                        Required = !param.IsOptional,
                        Description = paramDesc?.Description ?? GetParameterDescription(param),
                        Example = paramDesc?.Example,
                    }
                );
            }

            return parameters;
        }

        private string GenerateEnhancedResponseExample(
            MethodInfo method,
            ResponseExampleAttribute responseExampleAttr
        )
        {
            return null;
        }

        private string InferResponseExampleFromMethod(MethodInfo method)
        {
            // Default example
            return GenerateExampleJson(
                new ApiResponse<object>
                {
                    Success = true,
                    Data = new { result = "success", details = "Operation completed" },
                    Warnings = Array.Empty<string>(),
                }
            );
        }

        private string GenerateExampleFromType(Type type)
        {
            try
            {
                var instance = CreateExampleInstance(type);
                return GenerateExampleJson(instance);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error creating example for type {type.Name}: {ex}");
                return GenerateExampleJson(new { error = "Could not generate example" });
            }
        }

        private object CreateExampleInstance(Type type)
        {
            if (type == typeof(string))
                return "example string";
            if (type == typeof(int))
                return 42;
            if (type == typeof(bool))
                return true;
            if (type == typeof(long))
                return 123456789L;
            if (type == typeof(float))
                return 3.14f;
            if (type == typeof(double))
                return 3.14159;
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var array = Array.CreateInstance(elementType, 1);
                array.SetValue(CreateExampleInstance(elementType), 0);
                return array;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = type.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = Activator.CreateInstance(listType);
                listType
                    .GetMethod("Add")
                    ?.Invoke(list, new[] { CreateExampleInstance(elementType) });
                return list;
            }
            if (type.IsClass && type != typeof(object))
            {
                var instance = Activator.CreateInstance(type);

                // Set example values for properties
                foreach (
                    var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                )
                {
                    if (prop.CanWrite)
                    {
                        try
                        {
                            var exampleValue = CreateExampleInstance(prop.PropertyType);
                            prop.SetValue(instance, exampleValue);
                        }
                        catch
                        {
                            // Ignore properties we can't set
                        }
                    }
                }

                return instance;
            }

            return Activator.CreateInstance(type);
        }

        private string GenerateExampleJson(object example)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter() },
            };

            return JsonConvert.SerializeObject(example, settings);
        }

        private string GetTypeDisplayName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                var baseName = type.Name.Split('`')[0];
                return $"{baseName}<{string.Join(", ", genericArgs.Select(GetTypeDisplayName))}>";
            }
            return type.Name;
        }

        private string GetParameterDescription(ParameterInfo param)
        {
            var paramName = param.Name.ToLower();

            if (paramName.Contains("id"))
                return "Unique identifier";
            if (paramName.Contains("name"))
                return "Display name";
            if (paramName.Contains("type"))
                return "Type classification";
            if (paramName.Contains("count"))
                return "Number of items";

            return $"Parameter: {param.Name}";
        }

        public string GenerateMarkdown()
        {
            var docs = GenerateDocumentation();
            var sb = new StringBuilder();

            // Enhanced markdown generation with better formatting
            sb.AppendLine("# RimWorld REST API Documentation");
            sb.AppendLine();
            sb.AppendLine($"**Generated**: {docs.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC  ");
            sb.AppendLine($"**Version**: {docs.Version}  ");
            sb.AppendLine();

            foreach (var section in docs.Sections)
            {
                sb.AppendLine($"## {section.Name}");
                sb.AppendLine();
                sb.AppendLine(section.Description);
                sb.AppendLine();

                var groupedEndpoints = section.Endpoints.GroupBy(e => e.Category);

                foreach (var group in groupedEndpoints)
                {
                    sb.AppendLine($"### {group.Key}");
                    sb.AppendLine();

                    foreach (var endpoint in group)
                    {
                        sb.AppendLine($"#### `{endpoint.Method} {endpoint.Path}`");
                        sb.AppendLine();
                        sb.AppendLine(endpoint.Description);
                        sb.AppendLine();

                        if (!string.IsNullOrEmpty(endpoint.Notes))
                        {
                            sb.AppendLine($"> **Notes**: {endpoint.Notes}");
                            sb.AppendLine();
                        }

                        if (endpoint.Parameters.Any())
                        {
                            sb.AppendLine("**Parameters:**");
                            sb.AppendLine();
                            sb.AppendLine("| Name | Type | Required | Description | Example |");
                            sb.AppendLine("|------|------|:--------:|-------------|---------|");
                            foreach (var param in endpoint.Parameters)
                            {
                                var example = string.IsNullOrEmpty(param.Example)
                                    ? "*N/A*"
                                    : $"`{param.Example}`";
                                var required = param.Required ? "✅" : "❌";
                                sb.AppendLine(
                                    $"| `{param.Name}` | `{param.Type}` | {required} | {param.Description} | {example} |"
                                );
                            }
                            sb.AppendLine();
                        }

                        if (!string.IsNullOrEmpty(endpoint.RequestExample))
                        {
                            sb.AppendLine("**Request Example:**");
                            sb.AppendLine();
                            sb.AppendLine("```json");
                            sb.AppendLine(endpoint.RequestExample.Trim());
                            sb.AppendLine("```");
                            sb.AppendLine();
                        }

                        if (!string.IsNullOrEmpty(endpoint.ResponseExample))
                        {
                            sb.AppendLine("**Response Example:**");
                            sb.AppendLine();
                            sb.AppendLine("```json");
                            sb.AppendLine(endpoint.ResponseExample.Trim());
                            sb.AppendLine("```");
                            sb.AppendLine();
                        }

                        sb.AppendLine("---");
                        sb.AppendLine();
                    }
                }
            }

            return sb.ToString();
        }

        private List<DocumentedEndpoint> DocumentExtensionEndpointsWithExamples(string extensionId)
        {
            var endpoints = new List<DocumentedEndpoint>();

            try
            {
                // Get the extension
                var extension = _extensionRegistry.Extensions.FirstOrDefault(e =>
                    e.ExtensionId.Equals(extensionId, StringComparison.OrdinalIgnoreCase)
                );

                if (extension == null)
                {
                    LogApi.Warning($"Extension '{extensionId}' not found for documentation");
                    return endpoints;
                }

                LogApi.Info($"Documenting extension: {extension.ExtensionName}");

                // Use reflection to find controllers in the extension's assembly
                var extensionAssembly = extension.GetType().Assembly;

                // Find all controller types in the extension assembly
                var controllerTypes = extensionAssembly
                    .GetTypes()
                    .Where(t => t.Name.EndsWith("Controller") && !t.IsAbstract && !t.IsInterface)
                    .ToList();

                foreach (var controllerType in controllerTypes)
                {
                    try
                    {
                        endpoints.AddRange(
                            DocumentControllerWithExamples(controllerType, extensionId)
                        );
                    }
                    catch (Exception ex)
                    {
                        LogApi.Error(
                            $"Error documenting controller {controllerType.Name} for extension {extensionId}: {ex}"
                        );
                    }
                }

                LogApi.Info(
                    $"Documented {endpoints.Count} endpoints for extension '{extensionId}'"
                );
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error documenting extension '{extensionId}': {ex}");
            }

            return endpoints;
        }

        private List<DocumentedEndpoint> DocumentControllerWithExamples(
            Type controllerType,
            string extensionNamespace = null
        )
        {
            var endpoints = new List<DocumentedEndpoint>();

            var methods = controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m =>
                    m.GetCustomAttributes<RouteAttribute>().Any()
                    || m.GetCustomAttributes<GetAttribute>().Any()
                    || m.GetCustomAttributes<PostAttribute>().Any()
                    || m.GetCustomAttributes<PutAttribute>().Any()
                    || m.GetCustomAttributes<DeleteAttribute>().Any()
                );

            foreach (var method in methods)
            {
                var routeAttrs = method.GetCustomAttributes<RouteAttribute>();
                foreach (var attr in routeAttrs)
                {
                    var endpoint = CreateDocumentedEndpoint(method, attr);

                    // Apply extension namespace to path if provided
                    if (!string.IsNullOrEmpty(extensionNamespace))
                    {
                        endpoint.Path = $"/{extensionNamespace}{endpoint.Path}";
                    }

                    endpoints.Add(endpoint);
                }
            }

            return endpoints;
        }

        private string GenerateRequestExample(MethodInfo method)
        {
            return null;
        }
    }

    public class ApiDocumentation
    {
        public DateTime GeneratedAt { get; set; }
        public string BaseUrl { get; set; }
        public string Version { get; set; }
        public List<DocumentationSection> Sections { get; set; } = new List<DocumentationSection>();
    }

    public class DocumentationSection
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DocumentedEndpoint> Endpoints { get; set; } = new List<DocumentedEndpoint>();
    }

    public class DocumentedEndpoint
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } = "General";
        public string Notes { get; set; }
        public string RequestExample { get; set; }
        public string ResponseExample { get; set; }
        public List<EndpointParameter> Parameters { get; set; } = new List<EndpointParameter>();
        public string Controller { get; set; }
        public string Action { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string DeprecationNotice { get; set; }
        public string SinceVersion { get; set; }
    }

    public class EndpointParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Source { get; set; } // "query", "path", "body", "header"
        public bool Required { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
        public string DefaultValue { get; set; }
        public string[] AllowedValues { get; set; } = Array.Empty<string>();
        public ParameterValidation Validation { get; set; }
    }

    public class ParameterValidation
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string Pattern { get; set; }
        public object Minimum { get; set; }
        public object Maximum { get; set; }
    }
}
