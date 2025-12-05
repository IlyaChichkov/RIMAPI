using System;
using System.Collections.Generic;
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
    }
}
