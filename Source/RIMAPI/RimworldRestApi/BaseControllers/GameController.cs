using System;
using System.Net;
using System.Threading.Tasks;

namespace RimworldRestApi.Controllers
{
    public class GameController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public GameController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task GetGameState(HttpListenerContext context)
        {
            var gameState = _gameDataService.GetGameState();

            await HandleETagCaching(context, gameState, data =>
                GenerateHash(
                    data.GameTick,
                    data.ColonyWealth,
                    data.ColonistCount,
                    data.Storyteller
                )
            );
        }
    }
}