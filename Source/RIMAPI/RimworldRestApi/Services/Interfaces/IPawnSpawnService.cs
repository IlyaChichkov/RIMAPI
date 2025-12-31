using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IPawnSpawnService
    {
        ApiResult<PawnSpawnDto> SpawnPawn(PawnSpawnRequestDto request);
    }
}