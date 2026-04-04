using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IPawnJobService
    {
        ApiResult AssignJob(PawnJobRequestDto request);
        ApiResult AssignTendJob(MedicalTendRequestDto request);
        ApiResult AssignBedRest(MedicalBedRestRequestDto request);
    }
}
