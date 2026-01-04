using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RIMAPI.Models;
using UnityEngine;
using Verse;

namespace RIMAPI.Helpers
{
    public static class GlobalMapHelper
    {
        public static List<SettlementDto> GetSettlements()
        {
            return Find.WorldObjects.Settlements.Select(s => new SettlementDto
            {
                Id = s.ID,
                Name = s.Name,
                TileId = s.Tile,
                Faction = FactionDto.ToDto(s.Faction)
            }).ToList();
        }

        public static List<SettlementDto> GetPlayerSettlements()
        {
            return Find.WorldObjects.Settlements.Where(s => s.Faction.IsPlayer).Select(s => new SettlementDto
            {
                Id = s.ID,
                Name = s.Name,
                TileId = s.Tile,
                Faction = FactionDto.ToDto(s.Faction)
            }).ToList();
        }

        public static List<TileDto> GetWorldData()
        {
            var grid = Find.WorldGrid;
            var tiles = new List<TileDto>(grid.TilesCount);
            for (int i = 0; i < grid.TilesCount; i++)
            {
                tiles.Add(TileHelper.GetTile(i));
            }
            return tiles;
        }

        public static List<TileDto> GetTilesInRadius(int tileId, float radius)
        {
            var grid = Find.WorldGrid;
            var radiusInTiles = radius * 1.66f;

            var tiles = new List<TileDto>();
            for (int i = 0; i < grid.TilesCount; i++)
            {
                if (grid.ApproxDistanceInTiles(tileId, i) <= radiusInTiles)
                {
                    tiles.Add(TileHelper.GetTile(i));
                }
            }

            return tiles;
        }

        public static CoordinatesDto GetTileCoordinates(int tileId)
        {
            var grid = Find.WorldGrid;
            if (tileId < 0 || tileId >= grid.TilesCount)
            {
                return null;
            }

            var longLat = grid.LongLatOf(tileId);
            return new CoordinatesDto
            {
                Lat = longLat.y,
                Lon = longLat.x
            };
        }
    }
}
