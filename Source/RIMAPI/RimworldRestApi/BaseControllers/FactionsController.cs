using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Core;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class FactionsController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public FactionsController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task GetFactions(HttpListenerContext context)
        {
            var factions = _gameDataService.GetFactions();

            await HandleETagCaching(context, factions, data =>
                GenerateHash(data)
            );
        }

    }
}