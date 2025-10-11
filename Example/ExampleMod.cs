using System;
using System.Threading.Tasks;
using System.Net;
using Verse;

using RimworldRestApi.Core;

namespace ExampleMod.RimApi
{
    public class JobsModExtension : IRimApiExtension
    {
        public string ExtensionId => "jobs";
        public string ExtensionName => "Jobs Management System";
        public string Version => "1.0.0";

        public void RegisterEndpoints(IExtensionRouter router)
        {
            // Register custom endpoints
            router.Get("active", HandleGetActiveJobs);
            router.Get("queue", HandleGetJobQueue);
            router.Get("types", HandleGetJobTypes);

            Log.Message("JobsMod: Registered API endpoints with RIMAPI");
        }

        private async Task HandleGetActiveJobs(HttpListenerContext context)
        {
            // Example implementation
            var activeJobs = new[]
            {
                new { Id = 1, Type = "Mining", Colonist = "John", Progress = 0.75f },
                new { Id = 2, Type = "Construction", Colonist = "Sarah", Progress = 0.25f }
            };

            await ResponseBuilder.Success(context.Response, activeJobs);
        }

        private async Task HandleGetJobQueue(HttpListenerContext context)
        {
            // Your mod's logic here
            var queue = new { Pending = 5, Completed = 12 };
            await ResponseBuilder.Success(context.Response, queue);
        }

        private async Task HandleGetJobTypes(HttpListenerContext context)
        {
            var jobTypes = new[] { "Mining", "Construction", "Growing", "Research" };
            await ResponseBuilder.Success(context.Response, jobTypes);
        }
    }
}
