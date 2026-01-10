using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Helpers
{
    public static class ColonistsHelper
    {
        public static List<Pawn> GetColonistsList()
        {
#if RIMWORLD_1_5
            return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.ToList();
#elif RIMWORLD_1_6
            return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists.ToList();
#endif
        }

        public static PawnDto GetSingleColonistDto(int pawnId)
        {
            return GetColonistsList()
                    .Where(s => s.thingIDNumber == pawnId)
                    .Select(p => PawnHelper.PawnToDto(p))
                    .FirstOrDefault();
        }

        public static PawnDetailedRequestDto GetSingleColonistDetailedDto(int pawnId)
        {
            var colonist = GetColonistsList()
                    .Where(s => s.thingIDNumber == pawnId)
                    .FirstOrDefault();

            return new PawnDetailedRequestDto
            {
                Pawn = PawnHelper.PawnToDto(colonist),
                Detailes = PawnHelper.PawnToDetailedDto(colonist)
            };
        }

        public static List<PawnDto> GetColonistsDtos()
        {
            return GetColonistsList()
                    .Select(p => PawnHelper.PawnToDto(p))
                    .ToList();
        }

        public static List<PawnDetailedRequestDto> GetColonistsDetailedDtos()
        {
            var result = new List<PawnDetailedRequestDto>();

            var colonists = GetColonistsList();
            foreach (Pawn colonist in colonists)
            {
                result.Add(new PawnDetailedRequestDto
                {
                    Pawn = PawnHelper.PawnToDto(colonist),
                    Detailes = PawnHelper.PawnToDetailedDto(colonist)
                });
            }

            return result;
        }

        public static List<PawnPositionDto> GetColonistPositions()
        {
            var positions = new List<PawnPositionDto>();

            try
            {
                var map = Find.CurrentMap;
                if (map == null)
                    return positions;

                var freeColonists = map.mapPawns?.FreeColonists;
                if (freeColonists == null)
                    return positions;

                foreach (var pawn in freeColonists)
                {
                    if (pawn == null || !pawn.Spawned)
                        continue;

                    positions.Add(new PawnPositionDto
                    {
                        Id = pawn.thingIDNumber,
                        MapId = map.uniqueID,
                        X = pawn.Position.x,
                        Z = pawn.Position.z
                    });
                }
            }
            catch (Exception ex)
            {
                Core.LogApi.Error($"Error getting colonist positions - {ex.Message}");
            }

            return positions;
        }
    }
}
