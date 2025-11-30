using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Services
{
    public class FactionService : IFactionService
    {
        public FactionService() { }

        public ApiResult<List<FactionsDto>> GetFactions()
        {
            List<FactionsDto> factions = new List<FactionsDto>();
            try
            {
                Faction playerFaction = Find.FactionManager?.OfPlayer;

                if (Current.ProgramState != ProgramState.Playing || Find.FactionManager == null)
                {
                    return ApiResult<List<FactionsDto>>.Fail(
                        "Current program state is not \"Playing\""
                    );
                }

                factions = Find
                    .FactionManager.AllFactionsListForReading.Select(f => new FactionsDto
                    {
                        Def = f.def?.defName,
                        Name = f.Name,
                        IsPlayer = f.IsPlayer,
                        Relation = f.IsPlayer
                            ? string.Empty
                            : (
                                playerFaction != null
                                    ? playerFaction.RelationKindWith(f).ToString()
                                    : string.Empty
                            ),
                        Goodwill = f.IsPlayer ? 0 : (playerFaction?.GoodwillWith(f) ?? 0),
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                return ApiResult<List<FactionsDto>>.Fail(ex.Message);
            }
            return ApiResult<List<FactionsDto>>.Ok(factions);
        }
    }
}
