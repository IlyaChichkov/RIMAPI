
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IGameDataService
    {
        ApiResult<DefsDto> GetAllDefs(AllDefsRequestDto body);
    }
}