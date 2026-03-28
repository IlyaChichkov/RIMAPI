using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Services
{
    public class BillService : IBillService
    {
        private const int MaxBillsPerStack = 15;

        public BillService() { }

        public ApiResult<List<BillDto>> GetBills(int buildingId)
        {
            var workTable = FindWorkTable(buildingId);
            if (workTable == null)
                return ApiResult<List<BillDto>>.Fail($"Building {buildingId} not found or is not a work table");

            var bills = workTable.BillStack.Bills
                .OfType<Bill_Production>()
                .Select(b => BillHelper.ToDto(b))
                .Where(b => b != null)
                .ToList();

            return ApiResult<List<BillDto>>.Ok(bills);
        }

        public ApiResult<BillDto> GetBill(int buildingId, int billId)
        {
            var workTable = FindWorkTable(buildingId);
            if (workTable == null)
                return ApiResult<BillDto>.Fail($"Building {buildingId} not found or is not a work table");

            var bill = FindBill(workTable, billId);
            if (bill == null)
                return ApiResult<BillDto>.Fail($"Bill {billId} not found on building {buildingId}");

            return ApiResult<BillDto>.Ok(BillHelper.ToDto(bill));
        }

        public ApiResult<BillDto> CreateBill(int buildingId, CreateBillRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RecipeDefName))
                    return ApiResult<BillDto>.Fail("recipeDefName is required");

                var workTable = FindWorkTable(buildingId);
                if (workTable == null)
                    return ApiResult<BillDto>.Fail($"Building {buildingId} not found or is not a work table");

                if (workTable.BillStack.Bills.Count >= MaxBillsPerStack)
                    return ApiResult<BillDto>.Fail("Bill stack is full (max 15)");

                var recipe = DefDatabase<RecipeDef>.GetNamed(request.RecipeDefName, false);
                if (recipe == null)
                    return ApiResult<BillDto>.Fail($"Recipe not found: {request.RecipeDefName}");

                var bill = new Bill_Production(recipe);
                ApplyRequestFields(bill, request.RepeatMode, request.RepeatCount, request.TargetCount,
                    request.StoreMode, request.Suspended, request.PauseWhenSatisfied, request.UnpauseWhenYouHave,
                    request.IncludeEquipped, request.IncludeTainted, request.HpRange, request.QualityRange,
                    request.LimitToAllowedStuff, request.IngredientSearchRadius, request.AllowedSkillRange,
                    request.PawnRestrictionId, request.PlayerCustomName);

                workTable.BillStack.AddBill(bill);
                return ApiResult<BillDto>.Ok(BillHelper.ToDto(bill));
            }
            catch (Exception ex)
            {
                return ApiResult<BillDto>.Fail($"Failed to create bill: {ex.Message}");
            }
        }

        public ApiResult<BillDto> UpdateBill(int buildingId, int billId, UpdateBillRequest request)
        {
            try
            {
                var workTable = FindWorkTable(buildingId);
                if (workTable == null)
                    return ApiResult<BillDto>.Fail($"Building {buildingId} not found or is not a work table");

                var bill = FindBill(workTable, billId);
                if (bill == null)
                    return ApiResult<BillDto>.Fail($"Bill {billId} not found on building {buildingId}");

                ApplyRequestFields(bill, request.RepeatMode, request.RepeatCount, request.TargetCount,
                    request.StoreMode, request.Suspended, request.PauseWhenSatisfied, request.UnpauseWhenYouHave,
                    request.IncludeEquipped, request.IncludeTainted, request.HpRange, request.QualityRange,
                    request.LimitToAllowedStuff, request.IngredientSearchRadius, request.AllowedSkillRange,
                    request.PawnRestrictionId, request.PlayerCustomName);

                return ApiResult<BillDto>.Ok(BillHelper.ToDto(bill));
            }
            catch (Exception ex)
            {
                return ApiResult<BillDto>.Fail($"Failed to update bill: {ex.Message}");
            }
        }

        public ApiResult DeleteBill(int buildingId, int billId)
        {
            try
            {
                var workTable = FindWorkTable(buildingId);
                if (workTable == null)
                    return ApiResult.Fail($"Building {buildingId} not found or is not a work table");

                var bill = FindBill(workTable, billId);
                if (bill == null)
                    return ApiResult.Fail($"Bill {billId} not found on building {buildingId}");

                workTable.BillStack.Delete(bill);
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"Failed to delete bill: {ex.Message}");
            }
        }

        public ApiResult ReorderBill(int buildingId, int billId, int offset)
        {
            try
            {
                var workTable = FindWorkTable(buildingId);
                if (workTable == null)
                    return ApiResult.Fail($"Building {buildingId} not found or is not a work table");

                var bill = FindBill(workTable, billId);
                if (bill == null)
                    return ApiResult.Fail($"Bill {billId} not found on building {buildingId}");

                workTable.BillStack.Reorder(bill, offset);
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"Failed to reorder bill: {ex.Message}");
            }
        }

        public ApiResult SuspendBill(int buildingId, int billId, bool suspended)
        {
            try
            {
                var workTable = FindWorkTable(buildingId);
                if (workTable == null)
                    return ApiResult.Fail($"Building {buildingId} not found or is not a work table");

                var bill = FindBill(workTable, billId);
                if (bill == null)
                    return ApiResult.Fail($"Bill {billId} not found on building {buildingId}");

                bill.suspended = suspended;
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"Failed to suspend bill: {ex.Message}");
            }
        }

        public ApiResult ClearBills(int buildingId)
        {
            try
            {
                var workTable = FindWorkTable(buildingId);
                if (workTable == null)
                    return ApiResult.Fail($"Building {buildingId} not found or is not a work table");

                workTable.BillStack.Clear();
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"Failed to clear bills: {ex.Message}");
            }
        }

        public ApiResult<List<RecipeDto>> GetAvailableRecipes(int buildingId)
        {
            var workTable = FindWorkTable(buildingId);
            if (workTable == null)
                return ApiResult<List<RecipeDto>>.Fail($"Building {buildingId} not found or is not a work table");

            var recipes = DefDatabase<RecipeDef>.AllDefsListForReading
                .Where(r => r.recipeUsers != null && r.recipeUsers.Contains(workTable.def))
                .Select(r => BillHelper.ToRecipeDto(r))
                .Where(r => r != null)
                .ToList();

            return ApiResult<List<RecipeDto>>.Ok(recipes);
        }

        public ApiResult<List<WorkTableDto>> GetWorkTables(int mapId)
        {
            var map = Find.Maps.FirstOrDefault(m => m.uniqueID == mapId);
            if (map == null)
                return ApiResult<List<WorkTableDto>>.Fail($"Map {mapId} not found");

            var workTables = map.listerBuildings.allBuildingsColonist
                .OfType<Building_WorkTable>()
                .Select(wt => BillHelper.ToWorkTableDto(wt))
                .Where(wt => wt != null)
                .ToList();

            return ApiResult<List<WorkTableDto>>.Ok(workTables);
        }

        private Building_WorkTable FindWorkTable(int buildingId)
        {
            foreach (var map in Find.Maps)
            {
                var building = map.listerBuildings.allBuildingsColonist
                    .FirstOrDefault(b => b.thingIDNumber == buildingId);

                if (building is Building_WorkTable wt)
                    return wt;
            }
            return null;
        }

        private Bill_Production FindBill(Building_WorkTable workTable, int billId)
        {
            return workTable.BillStack.Bills
                .OfType<Bill_Production>()
                .FirstOrDefault(b =>
                {
                    if (LoadIDField == null) return false;
                    return (int)LoadIDField.GetValue(b) == billId;
                });
        }

        private static readonly FieldInfo LoadIDField = typeof(Bill).GetField("loadID", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo PawnRestrictionField = typeof(Bill).GetField("pawnRestriction", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo LabelField = typeof(Bill).GetField("label", BindingFlags.Instance | BindingFlags.NonPublic);

        private void ApplyRequestFields(Bill_Production bill,
            string repeatMode, int? repeatCount, int? targetCount,
            string storeMode, bool? suspended, bool? pauseWhenSatisfied, int? unpauseWhenYouHave,
            bool? includeEquipped, bool? includeTainted,
            Models.FloatRange hpRange, IntRangeDto qualityRange,
            bool? limitToAllowedStuff, float? ingredientSearchRadius, IntRangeDto allowedSkillRange,
            int? pawnRestrictionId, string playerCustomName)
        {
            if (!string.IsNullOrEmpty(repeatMode))
            {
                var mode = DefDatabase<BillRepeatModeDef>.GetNamed(repeatMode, false);
                if (mode != null)
                    bill.repeatMode = mode;
            }

            if (repeatCount.HasValue)
                bill.repeatCount = repeatCount.Value;

            if (targetCount.HasValue)
                bill.targetCount = targetCount.Value;

            if (!string.IsNullOrEmpty(storeMode))
            {
                var mode = DefDatabase<BillStoreModeDef>.GetNamed(storeMode, false);
                if (mode != null)
                    bill.SetStoreMode(mode);
            }

            if (suspended.HasValue)
                bill.suspended = suspended.Value;

            if (pauseWhenSatisfied.HasValue)
                bill.pauseWhenSatisfied = pauseWhenSatisfied.Value;

            if (unpauseWhenYouHave.HasValue)
                bill.unpauseWhenYouHave = unpauseWhenYouHave.Value;

            if (includeEquipped.HasValue)
                bill.includeEquipped = includeEquipped.Value;

            if (includeTainted.HasValue)
                bill.includeTainted = includeTainted.Value;

            if (hpRange != null)
                bill.hpRange = new Verse.FloatRange(hpRange.Min, hpRange.Max);

            if (qualityRange != null)
                bill.qualityRange = new QualityRange((QualityCategory)qualityRange.Min, (QualityCategory)qualityRange.Max);

            if (limitToAllowedStuff.HasValue)
                bill.limitToAllowedStuff = limitToAllowedStuff.Value;

            if (ingredientSearchRadius.HasValue)
                bill.ingredientSearchRadius = ingredientSearchRadius.Value;

            if (allowedSkillRange != null)
                bill.allowedSkillRange = new IntRange(allowedSkillRange.Min, allowedSkillRange.Max);

            if (pawnRestrictionId.HasValue && PawnRestrictionField != null)
            {
                foreach (var map in Find.Maps)
                {
                    var pawn = map.mapPawns.AllPawns.FirstOrDefault(p => p.thingIDNumber == pawnRestrictionId.Value);
                    if (pawn != null)
                    {
                        PawnRestrictionField.SetValue(bill, pawn);
                        break;
                    }
                }
            }

            if (playerCustomName != null && LabelField != null)
                LabelField.SetValue(bill, playerCustomName);
        }
    }
}
