using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RIMAPI.Models;
using Verse;

namespace RIMAPI.Helpers
{
    public static class SiteHelper
    {
        public static List<SiteDto> GetSites()
        {
            return Find.WorldObjects.Sites.Select(s => new SiteDto
            {
                Id = s.ID,
                Name = s.Label,
                Tile = s.Tile,
                Faction = FactionDto.ToDto(s.Faction)
            }).ToList();
        }
    }
}
