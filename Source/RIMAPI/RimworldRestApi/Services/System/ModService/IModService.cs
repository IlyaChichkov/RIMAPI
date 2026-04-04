
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IModService
    {
        ApiResult<List<ModInfoDto>> GetModsInfo();
        ApiResult ConfigureMods(ConfigureModsRequestDto body);
    }
}