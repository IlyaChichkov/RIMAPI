
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IGameDataService
    {
        void RefreshCache();
        void UpdateGameTick(int currentTick);
    }

    public interface IBuildingService
    {
        ApiResult<BuildingDto> GetBuildingInfo(int buildingId);
    }

    public interface IJobService { }
}
