
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IBillService
    {
        ApiResult<List<BillDto>> GetBills(int buildingId);
        ApiResult<BillDto> GetBill(int buildingId, int billId);
        ApiResult<BillDto> CreateBill(int buildingId, CreateBillRequest request);
        ApiResult<BillDto> UpdateBill(int buildingId, int billId, UpdateBillRequest request);
        ApiResult DeleteBill(int buildingId, int billId);
        ApiResult ReorderBill(int buildingId, int billId, int offset);
        ApiResult SuspendBill(int buildingId, int billId, bool suspended);
        ApiResult ClearBills(int buildingId);
        ApiResult<List<RecipeDto>> GetAvailableRecipes(int buildingId, bool onlyResearched = false);
        ApiResult<List<WorkTableDto>> GetWorkTables(int mapId);
    }
}
