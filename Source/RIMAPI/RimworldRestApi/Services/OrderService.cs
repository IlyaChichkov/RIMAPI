using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RIMAPI.Services;
using RimWorld;
using Verse;

public class OrderService : IOrderService
{
    public ApiResult DesignateArea(DesignateRequestDto request)
    {
        var map = MapHelper.GetMapByID(request.MapId);
        if (map == null) return ApiResult.Fail($"Map with id: {request.MapId} not found");

        IntVec3 start = new IntVec3(request.PointA.X, 0, request.PointA.Z);
        IntVec3 end = new IntVec3(request.PointB.X, 0, request.PointB.Z);
        CellRect rect = CellRect.FromLimits(start, end);

        int count = 0;

        foreach (IntVec3 c in rect)
        {
            if (!c.InBounds(map)) continue;

            if (request.Type.ToLower() == "mine")
            {
                // Add Mining Designation
                if (c.GetEdifice(map) != null && c.GetEdifice(map).def.mineable)
                {
                    map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.Mine));
                    count++;
                }
            }
            else if (request.Type.ToLower() == "deconstruct")
            {
                // Find buildings to deconstruct
                var things = c.GetThingList(map).ToList();
                foreach (var t in things)
                {
                    if (t.def.category == ThingCategory.Building)
                    {
                        map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.Deconstruct));
                        count++;
                    }
                }
            }
            else if (request.Type.ToLower() == "harvest")
            {
                var plants = c.GetThingList(map).Where(t => t.def.category == ThingCategory.Plant).Cast<Plant>();
                foreach (var p in plants)
                {
                    if (p.HarvestableNow && map.designationManager.DesignationOn(p, DesignationDefOf.HarvestPlant) == null)
                    {
                        map.designationManager.AddDesignation(new Designation(p, DesignationDefOf.HarvestPlant));
                        count++;
                    }
                }
            }
        }

        return ApiResult.Ok();
    }
}