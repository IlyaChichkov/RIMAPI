using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class OrderController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Post("/api/v1/order/designate/area")]
        public async Task DesignateArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DesignateRequestDto>();
            var result = _orderService.DesignateArea(body);
            await context.SendJsonResponse(result);
        }
    }
}