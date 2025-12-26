using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface ILordMakerService
    {
        ApiResult<LordCreateDto> CreateLord(LordCreateRequestDto request);
    }
}