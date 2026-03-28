using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class BillController
    {
        private readonly IBillService _billService;

        public BillController(IBillService billService)
        {
            _billService = billService;
        }

        [Get("/api/v1/buildings/bills")]
        [EndpointMetadata("List all bills on a work table")]
        public async Task GetBills(HttpListenerContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "building_id");
            var result = _billService.GetBills(buildingId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/buildings/bills")]
        [EndpointMetadata("Create a new bill on a work table")]
        public async Task CreateBill(HttpListenerContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "building_id");
            var body = await context.Request.ReadBodyAsync<CreateBillRequest>();
            var result = _billService.CreateBill(buildingId, body);
            await context.SendJsonResponse(result);
        }

        [Delete("/api/v1/buildings/bills")]
        [EndpointMetadata("Clear all bills from a work table")]
        public async Task ClearBills(HttpListenerContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "building_id");
            var result = _billService.ClearBills(buildingId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/buildings/bill")]
        [EndpointMetadata("Get a specific bill on a work table")]
        public async Task GetBill(HttpListenerContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "building_id");
            var billId = RequestParser.GetIntParameter(context, "bill_id");
            var result = _billService.GetBill(buildingId, billId);
            await context.SendJsonResponse(result);
        }

        [Put("/api/v1/buildings/bill")]
        [EndpointMetadata("Update a bill on a work table")]
        public async Task UpdateBill(HttpListenerContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "building_id");
            var billId = RequestParser.GetIntParameter(context, "bill_id");
            var body = await context.Request.ReadBodyAsync<UpdateBillRequest>();
            var result = _billService.UpdateBill(buildingId, billId, body);
            await context.SendJsonResponse(result);
        }

        [Delete("/api/v1/buildings/bill")]
        [EndpointMetadata("Delete a bill from a work table")]
        public async Task DeleteBill(HttpListenerContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "building_id");
            var billId = RequestParser.GetIntParameter(context, "bill_id");
            var result = _billService.DeleteBill(buildingId, billId);
            await context.SendJsonResponse(result);
        }

        [Put("/api/v1/buildings/bill/reorder")]
        [EndpointMetadata("Reorder a bill on a work table")]
        public async Task ReorderBill(HttpListenerContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "building_id");
            var billId = RequestParser.GetIntParameter(context, "bill_id");
            var body = await context.Request.ReadBodyAsync<BillReorderRequest>();
            var result = _billService.ReorderBill(buildingId, billId, body.Offset);
            await context.SendJsonResponse(result);
        }

        [Put("/api/v1/buildings/bill/suspend")]
        [EndpointMetadata("Suspend or resume a bill on a work table")]
        public async Task SuspendBill(HttpListenerContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "building_id");
            var billId = RequestParser.GetIntParameter(context, "bill_id");
            var body = await context.Request.ReadBodyAsync<BillSuspendRequest>();
            var result = _billService.SuspendBill(buildingId, billId, body.Suspended);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/buildings/recipes")]
        [EndpointMetadata("Get available recipes for a work table")]
        public async Task GetAvailableRecipes(HttpListenerContext context)
        {
            var buildingId = RequestParser.GetIntParameter(context, "building_id");
            var result = _billService.GetAvailableRecipes(buildingId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/map/work-tables")]
        [EndpointMetadata("Get all work tables on a map")]
        public async Task GetWorkTables(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _billService.GetWorkTables(mapId);
            await context.SendJsonResponse(result);
        }
    }
}
