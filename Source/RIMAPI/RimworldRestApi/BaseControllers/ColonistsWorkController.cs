using System;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class ColonistsWorkController : BaseController
    {
        private readonly IColonistService _colonistService;
        private readonly ICachingService _cachingService;

        public ColonistsWorkController(
            IColonistService colonistService,
            ICachingService cachingService
        )
        {
            _colonistService = colonistService;
            _cachingService = cachingService;
        }

        [Get("/api/v1/pawn/portrait/image")]
        public async Task GetPawnPortraitImage(HttpListenerContext context)
        {
            int pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            int width = RequestParser.GetIntParameter(context, "width");
            int height = RequestParser.GetIntParameter(context, "height");
            string direction = RequestParser.GetStringParameter(context, "direction");

            var result = _colonistService.GetPawnPortraitImage(pawnId, width, height, direction);

            await context.SendJsonResponse(result);
        }
    }
}
