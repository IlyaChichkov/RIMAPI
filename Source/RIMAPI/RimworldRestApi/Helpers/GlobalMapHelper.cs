using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RIMAPI.Models;
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
                Tile = s.Tile,
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
    }
}
