using System;

namespace RIMAPI.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EndpointDescriptionAttribute : Attribute
    {
        public string Description { get; }
        public string Category { get; set; } = "General";
        public string Notes { get; set; }

        public EndpointDescriptionAttribute(string description)
        {
            Description = description;
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
