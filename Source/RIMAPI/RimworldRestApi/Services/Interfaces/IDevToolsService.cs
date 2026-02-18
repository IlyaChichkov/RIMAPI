
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IDevToolsService
    {
        ApiResult<MaterialsAtlasList> GetMaterialsAtlasList();
        ApiResult MaterialsAtlasPoolClear();
        ApiResult ConsoleAction(DebugConsoleRequest body);
        ApiResult SetStuffColor(StuffColorRequest stuffColor);
    }
}
