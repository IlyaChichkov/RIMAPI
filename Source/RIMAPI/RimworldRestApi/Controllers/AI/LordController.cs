using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;
using RIMAPI.Http;

namespace RIMAPI.Controllers
{
    public class LordController
    {
        private readonly ILordMakerService _lordService;

        public LordController(ILordMakerService lordService)
        {
            _lordService = lordService;
        }

        [Post("/api/v1/lords/create")]
        public async Task CreateLord(RimApiContext context)
        {
            // Helper to parse body safely
            LordCreateRequestDto body;
            try
            {
                body = await context.Request.ReadBodyAsync<LordCreateRequestDto>();
            }
            catch
            {
                await context.SendJsonResponse(ApiResult.Fail("Invalid JSON body"));
                return;
            }

            var result = _lordService.CreateLord(body);
            await context.SendJsonResponse(result);
        }
    }
}