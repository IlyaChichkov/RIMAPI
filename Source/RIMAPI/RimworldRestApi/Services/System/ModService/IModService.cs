
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IModService
    {
        ApiResult<List<ModInfoDto>> GetModsInfo();
        ApiResult<ModInfoDto> GetModInfo(string packageId);
        ApiResult<string> GetModPreview(string packageId);
        ApiResult ConfigureMods(ConfigureModsRequestDto body);
    }
}