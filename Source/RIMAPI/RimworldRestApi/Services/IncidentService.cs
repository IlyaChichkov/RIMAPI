using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public class IncidentService : IIncidentService
    {
        public IncidentService() { }

        public ApiResult<IncidentsDto> GetIncidentsData(int mapId)
        {
            throw new System.NotImplementedException();
        }

        public ApiResult<List<LordDto>> GetLordsData(int mapId)
        {
            throw new System.NotImplementedException();
        }

        public ApiResult<QuestsDto> GetQuestsData(int mapId)
        {
            throw new System.NotImplementedException();
        }

        public ApiResult TriggerIncident(TriggerIncidentRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
