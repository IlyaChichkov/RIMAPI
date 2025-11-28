using System;

namespace RIMAPI.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RouteAttribute : Attribute
    {
        public string Method { get; }
        public string Pattern { get; }

        public RouteAttribute(string method, string pattern)
        {
            Method = method;
            Pattern = pattern;
        }
    }

    // Convenience attributes
    public class GetAttribute : RouteAttribute
    {
        public GetAttribute(string pattern)
            : base("GET", pattern) { }
    }

    public class PostAttribute : RouteAttribute
    {
        public PostAttribute(string pattern)
            : base("POST", pattern) { }
    }

    public class PutAttribute : RouteAttribute
    {
        public PutAttribute(string pattern)
            : base("PUT", pattern) { }
    }

    public class DeleteAttribute : RouteAttribute
    {
        public DeleteAttribute(string pattern)
            : base("DELETE", pattern) { }
    }
}
