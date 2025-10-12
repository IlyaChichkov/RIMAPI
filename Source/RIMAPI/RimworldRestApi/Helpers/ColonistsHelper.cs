using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RimWorld;
using RimworldRestApi.Models;
using UnityEngine;
using Verse;

namespace RimworldRestApi.Helpers
{
    public class ColonistsHelper
    {
        public List<ColonistDto> GetColonists()
        {
            var colonists = new List<ColonistDto>();

            try
            {
                var map = Find.CurrentMap;
                if (map == null) return colonists;

                var freeColonists = map.mapPawns?.FreeColonists;
                if (freeColonists == null) return colonists;

                foreach (var pawn in freeColonists)
                {
                    if (pawn == null) continue;

                    colonists.Add(new ColonistDto
                    {
                        Id = pawn.thingIDNumber,
                        Name = pawn.Name?.ToStringShort ?? "Unknown",
                        Gender = pawn.gender.ToString(),
                        Age = pawn.ageTracker.AgeBiologicalYears,
                        Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
                        Mood = pawn.needs?.mood?.CurLevelPercentage ?? 0.5f,
                        Position = new PositionDto
                        {
                            X = pawn.Position.x,
                            Y = pawn.Position.y,
                            Z = pawn.Position.z
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting colonists - {ex.Message}");
            }

            return colonists;
        }

        public List<ColonistDetailedDto> GetColonistsDetailed()
        {
            var colonists = new List<ColonistDetailedDto>();

            try
            {
                var map = Find.CurrentMap;
                if (map == null) return colonists;

                var freeColonists = map.mapPawns?.FreeColonists;
                if (freeColonists == null) return colonists;

                foreach (var pawn in freeColonists)
                {
                    if (pawn == null) continue;

                    colonists.Add(PawnToDetailedDto(pawn));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting detailed colonists - {ex.Message}");
            }

            return colonists;
        }

        public ColonistDetailedDto PawnToDetailedDto(Pawn pawn)
        {
            try
            {
                return new ColonistDetailedDto
                {
                    Id = pawn.thingIDNumber,
                    Name = pawn.Name?.ToStringShort ?? "Unknown",
                    Age = pawn.ageTracker?.AgeBiologicalYears ?? 0,
                    Gender = pawn.gender.ToString(),
                    Position = new PositionDto
                    {
                        X = pawn.Position.x,
                        Y = pawn.Position.z
                    },
                    Mood = (pawn.needs?.mood?.CurLevelPercentage ?? -1f) * 100,
                    Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
                    Hediffs = GetHediffs(pawn),
                    CurrentJob = pawn.CurJob?.def?.defName ?? "",
                    Traits = GetTraits(pawn),
                    WorkPriorities = GetWorkPriorities(pawn)
                };
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error converting pawn to DTO - {ex.Message}");
                return new ColonistDetailedDto { Id = pawn.thingIDNumber, Name = "Error" };
            }
        }

        public List<HediffDto> GetHediffs(Pawn pawn)
        {
            try
            {
                return pawn.health?.hediffSet?.hediffs?
                    .Where(h => h != null)
                    .Select(h => new HediffDto
                    {
                        Part = h.Part?.Label,
                        Label = h.Label
                    })
                    .ToList() ?? new List<HediffDto>();
            }
            catch
            {
                return new List<HediffDto>();
            }
        }

        public List<string> GetTraits(Pawn pawn)
        {
            try
            {
                return pawn.story?.traits?.allTraits?
                    .Where(t => t != null)
                    .Select(t => t.def.defName)
                    .ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public List<WorkPriorityDto> GetWorkPriorities(Pawn pawn)
        {
            var priorities = new List<WorkPriorityDto>();

            try
            {
                if (pawn.workSettings == null) return priorities;

                foreach (var workType in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    if (workType == null) continue;

                    var priority = pawn.workSettings.GetPriority(workType);
                    if (priority > 0)
                    {
                        priorities.Add(new WorkPriorityDto
                        {
                            WorkType = workType.defName,
                            Priority = priority
                        });
                    }
                }

                return priorities.OrderBy(p => p.Priority).ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error getting work priorities for pawn {pawn.thingIDNumber} - {ex.Message}");
                return priorities;
            }
        }

        public ColonistDetailedDto GetColonistInventory(int id)
        {
            throw new NotImplementedException();
        }
    }
}