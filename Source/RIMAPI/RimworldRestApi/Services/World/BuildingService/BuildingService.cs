using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Services
{
    public class BuildingService : IBuildingService
    {
        public BuildingService() { }

        public ApiResult<BuildingDto> GetBuildingInfo(int buildingId)
        {
            Building building = BuildingHelper.FindBuildingByID(buildingId);
            BuildingDto result = null;

            if (building == null)
            {
                return ApiResult<BuildingDto>.Fail("Building with this id wasn't found");
            }

            // Turret Info
            if (building is Building_Turret)
            {
                result = BuildingHelper.GetTurretInfo(building);
            }

            // Generator Info
            if (building.TryGetComp<CompPowerPlant>() != null)
            {
                result = BuildingHelper.GetPowerGeneratorInfo(building);
            }

            // TODO: Add other buildings info

            result = BuildingHelper.BuildingToDto(building);
            return ApiResult<BuildingDto>.Ok(result);
        }

        public ApiResult SetBuildingPower(int buildingId, bool powerOn)
        {
            Building building = BuildingHelper.FindBuildingByID(buildingId);
            if (building == null)
            {
                return ApiResult.Fail($"Building not found: {buildingId}");
            }

            bool success = BuildingHelper.SetBuildingPower(building, powerOn);
            if (!success)
            {
                return ApiResult.Fail($"Building {buildingId} has no flickable power component");
            }
            return ApiResult.Ok();
        }
    }
}
