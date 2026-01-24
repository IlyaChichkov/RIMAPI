using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;
using System;

namespace RIMAPI.Controllers
{
    public class ServerCacheController
    {
        private readonly ICachingService _cachingService;

        public ServerCacheController(ICachingService cachingService)
        {
            _cachingService = cachingService;
        }

        [Post("/api/v1/cache/enable")]
        public async Task EnableCache(HttpListenerContext context)
        {
            var result = _cachingService.SetEnabled(true);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/cache/disable")]
        public async Task DisableCache(HttpListenerContext context)
        {
            var result = _cachingService.SetEnabled(false);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/cache/status")]
        public async Task GetCacheStatus(HttpListenerContext context)
        {
            var isDetailed = RequestParser.GetBooleanParameter(context, "detailed", false);
            var stats = _cachingService.GetStatistics(isDetailed);
            var status = new { enabled = _cachingService.IsEnabled(), statistics = stats };

            await ResponseBuilder.SendSuccess(context.Response, status);
        }

        [Post("/api/v1/cache/clear")]
        public async Task ClearCache(HttpListenerContext context)
        {
            var result = _cachingService.Clear();
            await context.SendJsonResponse(result);
        }
    }
}