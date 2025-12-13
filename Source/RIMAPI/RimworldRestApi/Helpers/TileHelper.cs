using System.Linq;
using RIMAPI.Models;
using RimWorld.Planet;
using Verse;

namespace RIMAPI.Helpers
{
    public static class TileHelper
    {
        public static TileDto GetTile(int tileId)
        {
            var grid = Find.WorldGrid;
            if (tileId < 0 || tileId >= grid.TilesCount)
            {
                return null;
            }

            var tile = grid[tileId];
            return new TileDto
            {
                Id = tileId,
                Biome = tile.biome.defName,
                Elevation = tile.elevation,
                Hilliness = tile.hilliness.ToString(),
                Rainfall = tile.rainfall,
                Temperature = tile.temperature,
                Roads = tile.Roads?.Select(r => r.road.defName).ToList(),
                Rivers = tile.Rivers?.Select(r => r.river.defName).ToList()
            };
        }
    }
}
