
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IBuildingService
    {
        ApiResult<BuildingDto> GetBuildingInfo(int buildingId);
        ApiResult SetBuildingPower(int buildingId, bool powerOn);
    }
}
