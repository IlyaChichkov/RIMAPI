using System.Linq;
using RIMAPI.Models;
using RimWorld.Planet;
using UnityEngine;
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
            Vector2 longLat = grid.LongLatOf(tileId);
            return new TileDto
            {
                Id = tileId,
#if RIMWORLD_1_5
                Biome = tile.biome.defName,
#elif RIMWORLD_1_6
                Biome = tile.PrimaryBiome.defName,
#endif
                Elevation = tile.elevation,
                Lat = longLat.y,
                Lon = longLat.x,
                Hilliness = tile.hilliness.ToString(),
                Rainfall = tile.rainfall,
                Temperature = tile.temperature,
                Roads = tile.Roads?.Select(r => $"{r.neighbor}:{r.road.defName}").ToList(),
                Rivers = tile.Rivers?.Select(r => r.river.defName).ToList(),
                IsPolluted = ModsConfig.BiotechActive && tile.pollution > 0f,
                Pollution = ModsConfig.BiotechActive ? tile.pollution : 0f
            };
        }
    }
}
