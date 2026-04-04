using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;
using System;

namespace RIMAPI.Controllers
{
    public class TradeController
    {
        private readonly ITradeService _tradeService;
        private readonly ICachingService _cachingService;

        public TradeController(ITradeService tradeService, ICachingService cachingService)
        {
            _tradeService = tradeService;
            _cachingService = cachingService;
        }

        [Get("/api/v1/traders/defs")]
        public async Task GetTradersDefs(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/traders/defs",
                dataFactory: () => Task.FromResult(_tradeService.GetAllTraderDefs()),
                expiration: TimeSpan.FromMinutes(5),
                priority: CachePriority.Normal,
                expirationType: CacheExpirationType.Absolute
            );
        }
    }
}