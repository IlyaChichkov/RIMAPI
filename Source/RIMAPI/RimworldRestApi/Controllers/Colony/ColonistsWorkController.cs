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
    }
}
