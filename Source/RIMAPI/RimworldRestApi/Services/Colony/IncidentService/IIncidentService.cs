
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IIncidentService
    {
        ApiResult<QuestsDto> GetQuestsData(int mapId);
        ApiResult<IncidentsDto> GetIncidentsData(int mapId);
        ApiResult<List<LordDto>> GetLordsData(int mapId);
        ApiResult TriggerIncident(TriggerIncidentRequestDto request);
        ApiResult<IncidentChanceDto> GetIncidentChance(IncidentChanceRequestDto request);
        ApiResult<List<IncidentWeightDto>> GetTopIncidents(int limit = 10);

    }
}