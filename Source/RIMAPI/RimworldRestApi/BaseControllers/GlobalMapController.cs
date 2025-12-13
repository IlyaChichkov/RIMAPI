using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class GlobalMapController : BaseController
    {
        private readonly IGlobalMapService _globalMapService;
        private readonly ICachingService _cachingService;

        public GlobalMapController(IGlobalMapService globalMapService, ICachingService cachingService)
        {
            _globalMapService = globalMapService;
            _cachingService = cachingService;
        }

        [Get("/api/v1/world/caravans")]
        public async Task GetCaravans(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "caravans",
                () => Task.FromResult(_globalMapService.GetCaravans()),
                expiration: TimeSpan.FromSeconds(3),
                expirationType: CacheExpirationType.GameTick
            );
        }

        [Get("/api/v1/world/settlements")]
        public async Task GetSettlements(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "settlements",
                () => Task.FromResult(_globalMapService.GetSettlements()),
                expiration: TimeSpan.FromSeconds(10),
                expirationType: CacheExpirationType.GameTick
            );
        }

        [Get("/api/v1/world/sites")]
        public async Task GetSites(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "sites",
                () => Task.FromResult(_globalMapService.GetSites()),
                expiration: TimeSpan.FromSeconds(5),
                expirationType: CacheExpirationType.GameTick
            );
        }

        [Get("/api/v1/world/tile")]
        public async Task GetTile(HttpListenerContext context)
        {
            var tileId = RequestParser.GetIntParameter(context, "id");
            await _cachingService.CacheAwareResponseAsync(
                context,
                $"tile_{tileId}",
                () => Task.FromResult(_globalMapService.GetTile(tileId)),
                expiration: TimeSpan.FromSeconds(120),
                expirationType: CacheExpirationType.Absolute
            );
        }
    }
}
