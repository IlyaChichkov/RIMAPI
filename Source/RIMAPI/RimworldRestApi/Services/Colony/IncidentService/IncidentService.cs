using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace RIMAPI.Services
{
    public class IncidentService : IIncidentService
    {
        public IncidentService() { }

        public ApiResult<IncidentsDto> GetIncidentsData(int mapId)
        {
            try
            {
                Map map = MapHelper.GetMapByID(mapId);
                var result = new IncidentsDto { Incidents = GameEventsHelper.GetIncidentsLog(map) };
                return ApiResult<IncidentsDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResult<IncidentsDto>.Fail(ex.Message);
            }
        }

        public ApiResult<List<LordDto>> GetLordsData(int mapId)
        {
            try
            {
                List<LordDto> lordDtos = new List<LordDto>();
                foreach (Lord item in Find.CurrentMap.lordManager.lords)
                {
                    lordDtos.Add(LordDto.ToDto(item));
                }
                return ApiResult<List<LordDto>>.Ok(lordDtos);
            }
            catch (Exception ex)
            {
                return ApiResult<List<LordDto>>.Fail(ex.Message);
            }
        }

        public ApiResult<QuestsDto> GetQuestsData(int mapId)
        {
            try
            {
                Map map = MapHelper.GetMapByID(mapId);
                return ApiResult<QuestsDto>.Ok(GameEventsHelper.GetQuestsDto(map));
            }
            catch (Exception ex)
            {
                return ApiResult<QuestsDto>.Fail(ex.Message);
            }
        }

        public ApiResult TriggerIncident(TriggerIncidentRequestDto request)
        {
            try
            {
                IncidentDef incident = DefDatabase<IncidentDef>.GetNamed(request.Name);

                IncidentParms parms = null;
                if (request.IncidentParms == null)
                {
                    parms = StorytellerUtility.DefaultParmsNow(incident.category, Find.CurrentMap);
                    parms.forced = true;
                }
                else
                {
                    parms = DefDatabaseHelper.IncidentParmsFromDto(
                        request.IncidentParms,
                        Find.CurrentMap
                    );
                }

                incident.Worker.TryExecute(parms);
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult<IncidentChanceDto> GetIncidentChance(IncidentChanceRequestDto request)
        {
            try
            {
                IncidentDef incidentDef = DefDatabase<IncidentDef>.GetNamed(request.IncidentDefName);
                if (incidentDef == null)
                {
                    return ApiResult<IncidentChanceDto>.Fail($"Incident '{request.IncidentDefName}' not found in database.");
                }

                var storyteller = Find.Storyteller;
                if (storyteller == null)
                {
                    return ApiResult<IncidentChanceDto>.Fail("Storyteller not found.");
                }

                // Determine Target (Default to CurrentMap)
                IIncidentTarget target = Find.CurrentMap;
                if (target == null)
                {
                    return ApiResult<IncidentChanceDto>.Fail("No active map found to calculate incident chance.");
                }

                var result = new IncidentChanceDto
                {
                    Value = 0f
                };

                float totalChance = 0f;

                // Iterate ALL Storyteller Components
                // We removed the 'if (comp.props is ...)' filter. 
                // Most incidents (like ResourcePodCrash) are handled by generic 'CategoryMTB' components, 
                // not specific 'OnOffCycle' ones. We simply ask every component for its chance.
                foreach (var comp in storyteller.storytellerComps)
                {
                    // Use Harmony Traverse to access the protected method:
                    // protected virtual float IncidentChanceFinal(IncidentDef def, IIncidentTarget target)
                    float chance = Traverse.Create(comp)
                                           .Method("IncidentChanceFinal", incidentDef, target)
                                           .GetValue<float>();

                    totalChance += chance;
                }

                result.Value = totalChance;
                return ApiResult<IncidentChanceDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResult<IncidentChanceDto>.Fail($"Error calculating chance: {ex.Message}");
            }
        }


        public ApiResult<List<IncidentWeightDto>> GetTopIncidents(int limit = 10)
        {
            try
            {
                var target = Find.CurrentMap;
                if (target == null) return ApiResult<List<IncidentWeightDto>>.Fail("No active map.");

                var storyteller = Find.Storyteller;
                if (storyteller == null) return ApiResult<List<IncidentWeightDto>>.Fail("No storyteller.");

                var results = new List<IncidentWeightDto>();

                // Iterate through EVERY possible incident in the database
                foreach (var def in DefDatabase<IncidentDef>.AllDefs)
                {
                    // Optimization: Skip hidden or invalid defs immediately
                    if (def.TargetAllowed(target) == false) continue;

                    // Check if the incident is actually possible right now
                    // (e.g. checks biome, temperature, population, recent firing)
                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(def.category, target);
                    if (!def.Worker.CanFireNow(parms))
                    {
                        continue;
                    }

                    float totalChance = 0f;
                    foreach (var comp in storyteller.storytellerComps)
                    {
                        // Reflection to call protected 'IncidentChanceFinal'
                        float chance = Traverse.Create(comp)
                                               .Method("IncidentChanceFinal", def, target)
                                               .GetValue<float>();

                        // If this component creates a detailed explanation, use that (optional optimization)
                        // but usually just getting the float is enough.
                        if (chance > 0f)
                        {
                            totalChance += chance;
                        }
                    }

                    if (totalChance > 0f)
                    {
                        results.Add(new IncidentWeightDto
                        {
                            DefName = def.defName,
                            Label = def.label ?? def.defName,
                            Category = def.category?.defName ?? "Unknown",
                            CurrentWeight = totalChance
                        });
                    }
                }

                var topIncidents = results
                    .OrderByDescending(x => x.CurrentWeight)
                    .Take(limit)
                    .ToList();

                return ApiResult<List<IncidentWeightDto>>.Ok(topIncidents);
            }
            catch (System.Exception ex)
            {
                return ApiResult<List<IncidentWeightDto>>.Fail(ex.Message);
            }
        }
    }
}
