using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using Verse;
using Verse.AI.Group; // Needed for Lord, LordJob, LordMaker

namespace RIMAPI.Services
{
    public class LordMakerService : ILordMakerService
    {
        public ApiResult<LordCreateDto> CreateLord(LordCreateRequestDto request)
        {
            try
            {
                // 1. Resolve Map
                Map map = null;
                if (!string.IsNullOrEmpty(request.MapId) && int.TryParse(request.MapId, out int mapId))
                {
                    map = MapHelper.GetMapByID(mapId);
                }
                if (map == null) map = Find.CurrentMap;
                if (map == null) return ApiResult<LordCreateDto>.Fail("Could not determine map.");

                // 2. Resolve Faction
                Faction faction = Faction.OfPlayer;
                if (!string.IsNullOrEmpty(request.Faction))
                {
                    faction = Find.FactionManager.AllFactionsListForReading
                        .FirstOrDefault(f => f.def.defName == request.Faction || f.Name == request.Faction);
                }
                if (faction == null) return ApiResult<LordCreateDto>.Fail($"Faction '{request.Faction}' not found.");

                // 3. Resolve Pawns (The Squad Members)
                List<Pawn> squadPawns = new List<Pawn>();
                if (request.PawnIds != null)
                {
                    foreach (int id in request.PawnIds)
                    {
                        Pawn p = ColonistsHelper.FindPawnById(id.ToString());
                        if (p != null)
                        {
                            // If pawn is already in a lord, we might need to remove them first
                            if (p.GetLord() != null)
                            {
                                p.GetLord().RemovePawn(p);
                            }
                            squadPawns.Add(p);
                        }
                    }
                }

                if (squadPawns.Count == 0)
                    return ApiResult<LordCreateDto>.Fail("No valid pawns found to form a Lord.");

                // 4. Create the LordJob based on request
                LordJob lordJob = null;
                string jobType = request.JobType ?? "AssaultColony";

                switch (jobType.ToLower())
                {
                    case "assaultcolony":
                        // Standard Raid AI: Attack random buildings/people, don't flee easily
                        lordJob = new LordJob_AssaultColony(faction, canKidnap: false, canTimeoutOrFlee: false);
                        break;

                    case "assaultthings":
                        // Attack specific targets
                        List<Thing> targets = new List<Thing>();
                        if (request.TargetIds != null)
                        {
                            foreach (int tId in request.TargetIds)
                            {
                                // Reuse FindPawnById as targets are usually pawns
                                Pawn target = ColonistsHelper.FindPawnById(tId.ToString());
                                if (target != null) targets.Add(target);
                            }
                        }

                        if (targets.Count == 0)
                            return ApiResult<LordCreateDto>.Fail("Job 'AssaultThings' requires valid TargetIds.");

                        lordJob = new LordJob_AssaultThings(faction, targets);
                        break;

                    case "defendpoint":
                        // Guard a specific spot
                        if (request.Position == null)
                            return ApiResult<LordCreateDto>.Fail("Job 'DefendPoint' requires a Position.");

                        IntVec3 point = new IntVec3(request.Position.X, request.Position.Y, request.Position.Z);
                        lordJob = new LordJob_DefendPoint(point);
                        break;

                    default:
                        return ApiResult<LordCreateDto>.Fail($"Unknown LordJob type: {jobType}");
                }

                // 5. Make the Lord
                // This static method handles adding the lord to the map and assigning the job
                Lord lord = LordMaker.MakeNewLord(faction, lordJob, map, squadPawns);

                LogApi.Info($"[LordMaker] Created Lord {lord.loadID} with job {jobType} for {squadPawns.Count} pawns.");

                var result = new LordCreateDto
                {
                    LordId = lord.loadID,
                    MemberCount = squadPawns.Count
                };
                return ApiResult<LordCreateDto>.Ok(result);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error creating lord: {ex}");
                return ApiResult<LordCreateDto>.Fail(ex.Message);
            }
        }
    }
}