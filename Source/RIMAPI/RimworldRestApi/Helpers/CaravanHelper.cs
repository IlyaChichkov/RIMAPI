using System.Collections.Generic;
using System.Linq;
using RIMAPI.Models;
using RimWorld.Planet;
using Verse;

namespace RIMAPI.Helpers
{
    public static class CaravanHelper
    {
        public static List<Caravan> GetAllCaravans()
        {
            return Find.WorldObjects.Caravans;
        }

        public static Caravan GetCaravanById(int id)
        {
            return GetAllCaravans()
                       .Where(s => s.ID == id)
                       .FirstOrDefault();
        }

        public static List<CaravanDto> GetCaravans()
        {
            var caravanDtos = new List<CaravanDto>();
            foreach (var caravan in GetAllCaravans())
            {
                var caravanDto = new CaravanDto
                {
                    Id = caravan.ID,
                    Name = caravan.Name,
                    IsPlayerControlled = caravan.IsPlayerControlled,
                    Tile = caravan.Tile,
                    Position = new PositionDto { X = (int)caravan.DrawPos.x, Y = (int)caravan.DrawPos.y, Z = (int)caravan.DrawPos.z },
                    Pawns = caravan.PawnsListForReading.Select(p => new PawnDto { Id = p.thingIDNumber, Name = p.Name.ToStringShort }).ToList(),
                    Items = caravan.AllThings.Select(ResourcesHelper.ThingToDto).ToList(),
                    MassUsage = caravan.MassUsage,
                    MassCapacity = caravan.MassCapacity,
                    Forageability = caravan.Biome.forageability == 0f ? "Not Forageable" : "Forageable",
                    Visibility = caravan.Visibility.ToString(),
                    DaysToArrive = (float)CaravanArrivalTimeEstimator
                    .EstimatedTicksToArrive(
                        caravan, allowCaching: true
                    ) / 60000f
                };
                caravanDtos.Add(caravanDto);
            }
            return caravanDtos;
        }

        /// <summary>
        /// Returns an ordered list of Tile IDs representing the full path 
        /// from the caravan's current location to its destination.
        /// </summary>
        public static List<int> GetFullCaravanPath(Caravan caravan)
        {
            // Initialize list with explicit capacity to prevent resizing
            List<int> pathTiles = new List<int>(128);

            if (caravan == null) return pathTiles;

            // If the caravan isn't moving, the path is just the current tile.
            if (!caravan.pather.Moving)
            {
                return pathTiles;
            }

            // 2. Add the immediate next tile (the one being animated towards)
            // The pather 'consumes' a node from the WorldPath and stores it in 'nextTile'
            // while the caravan physically travels there.
            int nextTile = caravan.pather.nextTile;

            // Ensure the next tile is valid and distinct from current (sanity check)
            if (nextTile >= 0 && nextTile != caravan.Tile)
            {
                pathTiles.Add(nextTile);
            }

            // 3. Add the remaining nodes from the WorldPath buffer
            WorldPath curPath = caravan.pather.curPath;

            if (curPath != null && curPath.Found && curPath.NodesLeftCount > 0)
            {
                // WorldPath stores nodes internally in a reversed list (Destination -> Start),
                // but the Peek(i) method abstracts this logic for us.
                // Peek(0) is the next immediate step in the buffer.
                // Peek(NodesLeftCount - 1) is the final destination.
                for (int i = 0; i < curPath.NodesLeftCount; i++)
                {
                    int tileId = curPath.Peek(i);
                    pathTiles.Add(tileId);
                }
            }

            return pathTiles;
        }
    }
}
