using System.Collections.Generic;
using System.Linq;
using RIMAPI.Models;
using RimWorld.Planet;
using Verse;

namespace RIMAPI.Helpers
{
    public static class CaravanHelper
    {
        public static List<CaravanDto> GetCaravans()
        {
            var caravanDtos = new List<CaravanDto>();
            foreach (var caravan in Find.WorldObjects.Caravans)
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
                    Visibility = caravan.Visibility.ToString()
                };
                caravanDtos.Add(caravanDto);
            }
            return caravanDtos;
        }
    }
}
