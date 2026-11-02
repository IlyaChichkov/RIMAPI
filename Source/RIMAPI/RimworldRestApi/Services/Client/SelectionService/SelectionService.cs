using System;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Services
{
    public class SelectionService : ISelectionService
    {
        public SelectionService() { }

        public ApiResult Select(string objectType, int id)
        {
            try
            {
                switch (objectType)
                {
                    case "item":
                        var item = Find
                            .CurrentMap.listerThings.AllThings.Where(p => p.thingIDNumber == id)
                            .FirstOrDefault();
                        Find.Selector.Select(item);
                        break;
                    case "pawn":
                        var pawn = PawnHelper.FindPawnById(id);
                        Find.Selector.Select(pawn);
                        break;
                    case "building":
                        var building = BuildingHelper.FindBuildingByID(id);
                        Find.Selector.Select(building);
                        break;
                    default:
                        return ApiResult.Fail($"Tried to select unknown object type: {objectType}");
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult SelectArea(SelectAreaRequestDto body)
        {
            try
            {
                if (body?.PositionA == null || body.PositionB == null)
                {
                    return ApiResult.ValidationFail("position_a and position_b are required");
                }

                IntVec3 posA = new IntVec3(body.PositionA.X, body.PositionA.Y, body.PositionA.Z);
                IntVec3 posB = new IntVec3(body.PositionB.X, body.PositionB.Y, body.PositionB.Z);

                Map map = Find.CurrentMap;
                if (map == null)
                {
                    return ApiResult.Fail("No current map");
                }

                // Selector.Select only accepts things, zones and plans; select what the rectangle contains
                CellRect rect = CellRect.FromLimits(posA, posB).ClipInsideMap(map);
                Find.Selector.ClearSelection();
                var things = rect.Cells.SelectMany(c => map.thingGrid.ThingsListAt(c)).Distinct().ToList();
                foreach (Thing thing in things)
                {
                    if (ThingSelectionUtility.SelectableByMapClick(thing))
                        Find.Selector.Select(thing, playSound: false, forceDesignatorDeselect: false);
                }

                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error selecting area: {ex}");
                return ApiResult.Fail($"Failed to select area: {ex.Message}");
            }
        }

        public ApiResult DeselectAll()
        {
            try
            {
                Find.Selector.ClearSelection();
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error deselecting all: {ex}");
                return ApiResult.Fail($"Failed to deselect: {ex.Message}");
            }
        }
    }
}
