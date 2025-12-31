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
        string type = request.Type.ToLower(); // Cache lowercase type

        foreach (IntVec3 c in rect)
        {
            if (!c.InBounds(map)) continue;

            if (type == "mine")
            {
                // Add Mining Designation
                var edifice = c.GetEdifice(map);
                if (edifice != null && edifice.def.mineable && map.designationManager.DesignationAt(c, DesignationDefOf.Mine) == null)
                {
                    map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.Mine));
                    count++;
                }
            }
            else if (type == "deconstruct")
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
            else if (type == "harvest")
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
            else if (type == "hunt")
            {
                // Find animals to hunt
                var pawns = c.GetThingList(map).OfType<Pawn>();
                foreach (var p in pawns)
                {
                    // Check if it's an animal, not our colony pet, and not already marked
                    if (p.RaceProps.Animal && p.Faction != Faction.OfPlayer && map.designationManager.DesignationOn(p, DesignationDefOf.Hunt) == null)
                    {
                        map.designationManager.AddDesignation(new Designation(p, DesignationDefOf.Hunt));
                        count++;
                    }
                }
            }
            else if (type == "remove-all")
            {
                var things = c.GetThingList(map);
                foreach (var t in things)
                {
                    map.designationManager.RemoveAllDesignationsOn(t);
                }
                count++;
            }
        }

        return ApiResult.Ok();
    }
}