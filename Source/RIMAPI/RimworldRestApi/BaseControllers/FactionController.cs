using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class FactionController : RequestParser
    {
        private readonly IFactionService _factionService;

        public FactionController(IFactionService factionService)
        {
            _factionService = factionService;
        }

        [Get("/api/v1/factions")]
        public async Task GetCurrentMapDatetime(HttpListenerContext context)
        {
            var result = _factionService.GetFactions();
            await context.SendJsonResponse(result);
        }
    }
}
