


using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface ITradeService
    {
        ApiResult<List<TraderKindDto>> GetAllTraderDefs();
    }
}
