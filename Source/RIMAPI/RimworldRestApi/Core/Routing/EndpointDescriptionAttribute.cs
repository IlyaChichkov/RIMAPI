using System;

namespace RIMAPI.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EndpointMetadataAttribute : Attribute
    {
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Category { get; set; } = "General";
        public string[] Tags { get; set; } = Array.Empty<string>();
        public bool IsDeprecated { get; set; }
        public string DeprecationReason { get; set; }
        public string ExampleRequest { get; set; }
        public string ExampleResponse { get; set; }

        public EndpointMetadataAttribute(string description)
        {
            Description = description;
        }

        public EndpointMetadataAttribute(string description, string[] tags)
        {
            Description = description;
            Tags = tags;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ResponseExampleAttribute : Attribute
    {
        public Type ResponseType { get; }
        public string ExampleJson { get; set; }

        public ResponseExampleAttribute(Type responseType)
        {
            ResponseType = responseType;
        }

        public ResponseExampleAttribute(string exampleJson)
        {
            ExampleJson = exampleJson;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterDescriptionAttribute : Attribute
    {
        public string Description { get; }
        public string Example { get; set; }

        public ParameterDescriptionAttribute(string description)
        {
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class ModelDescriptionAttribute : Attribute
    {
        public string Description { get; }
        public string Example { get; set; }

        public ModelDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
