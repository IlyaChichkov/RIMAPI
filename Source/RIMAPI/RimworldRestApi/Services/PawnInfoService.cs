using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Services
{
    public class PawnInfoService : IPawnInfoService
    {
        // --- 1. Get List of Pawns on Map ---
        public ApiResult<List<PawnDto>> GetPawnsOnMap(int mapId)
        {
            try
            {
                var map = MapHelper.GetMapByID(mapId);
                if (map == null) return ApiResult<List<PawnDto>>.Fail($"Map {mapId} not found.");

                var result = new List<PawnDto>();

                // Get ALL pawns (colonists, prisoners, enemies, animals, mechs)
                foreach (var pawn in map.mapPawns.AllPawns)
                {
                    result.Add(PawnHelper.PawnToDto(pawn));
                }

                return ApiResult<List<PawnDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error getting pawns on map: {ex}");
                return ApiResult<List<PawnDto>>.Fail(ex.Message);
            }
        }

        // --- 2. Get Detailed Pawn Info ---
        public ApiResult<PawnDetailedDto> GetPawnDetails(int pawnId)
        {
            try
            {
                var pawn = PawnHelper.FindPawnById(pawnId);
                if (pawn == null) return ApiResult<PawnDetailedDto>.Fail($"Pawn {pawnId} not found.");

                var details = PawnHelper.PawnToDetailedDto(pawn);
                return ApiResult<PawnDetailedDto>.Ok(details);
            }
            catch (Exception ex)
            {
                return ApiResult<PawnDetailedDto>.Fail(ex.Message);
            }
        }

        public ApiResult<PawnInventoryDto> GetPawnInventory(int pawnId)
        {
            try
            {
                var pawn = PawnHelper.FindPawnById(pawnId);
                if (pawn == null) return ApiResult<PawnInventoryDto>.Fail($"Pawn {pawnId} not found.");

                var inventory = PawnHelper.GetPawnInventory(pawn);

                return ApiResult<PawnInventoryDto>.Ok(inventory);
            }
            catch (Exception ex)
            {
                return ApiResult<PawnInventoryDto>.Fail(ex.Message);
            }
        }

        // --- Internal Mappers ---

        private PawnDto MapPawnToSummary(Pawn pawn)
        {
            return new PawnDto
            {
                Id = pawn.thingIDNumber,
                Name = pawn.Name?.ToStringShort ?? pawn.Label,
                Gender = pawn.gender.ToString(),
                Age = pawn.ageTracker.AgeBiologicalYears,
                Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 0f,
                Mood = pawn.needs?.mood?.CurLevelPercentage ?? 0f, // Null for animals/mechs
                Hunger = pawn.needs?.food?.CurLevelPercentage ?? 0f,
                Position = new PositionDto { X = pawn.Position.x, Y = pawn.Position.y, Z = pawn.Position.z }
            };
        }
    }
}