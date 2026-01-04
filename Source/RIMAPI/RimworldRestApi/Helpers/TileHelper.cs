using System.Linq;
using RIMAPI.Models;
using RimWorld;
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

        public static TileDetailsDto GetTileDetails(int tileId)
        {
            var grid = Find.WorldGrid;
            if (tileId < 0 || tileId >= grid.TilesCount)
            {
                return null;
            }

            var tile = grid[tileId];
            var baseDto = GetTile(tileId);

            // Movement Difficulty  
            float movementDifficulty = 0;

            movementDifficulty = WorldPathGrid.CalculatedMovementDifficultyAt(tileId, false)
                            * Find.WorldGrid.GetRoadMovementDifficultyMultiplier(tileId, tileId);

            // Growing Period  
            string growingPeriod = Zone_Growing.GrowingQuadrumsDescription(tileId);

            // Forageability (count of forageable plants)  
            float forageablePlantCount = 0;

#if RIMWORLD_1_5
            forageablePlantCount = tile.biome.forageability;
#elif RIMWORLD_1_6
            forageablePlantCount = tile.PrimaryBiome.forageability;
#endif

            return new TileDetailsDto
            {
                Id = baseDto.Id,
                Biome = baseDto.Biome,
                Elevation = baseDto.Elevation,
                Lat = baseDto.Lat,
                Lon = baseDto.Lon,
                Hilliness = baseDto.Hilliness,
                Rainfall = baseDto.Rainfall,
                Temperature = baseDto.Temperature,
                Roads = baseDto.Roads,
                Rivers = baseDto.Rivers,
                IsPolluted = baseDto.IsPolluted,
                Pollution = baseDto.Pollution,

                TimeZone = GenDate.TimeZoneAt(baseDto.Lon).ToString(),
                Forageability = forageablePlantCount,
                MovementDifficulty = movementDifficulty,
                GrowingPeriod = growingPeriod,
                StoneTypes = Find.World.NaturalRockTypesIn(tileId).Select(s => s.defName).ToList()
            };
        }
    }
}
