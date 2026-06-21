using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Models.UI;

namespace RIMAPI.Services.Interfaces
{
    public interface IUIService
    {
        ApiResult<List<AlertDto>> GetActiveAlerts();
        ApiResult OpenTab(string tabName);
        ApiResult SendLetterSimple(SendLetterRequestDto body);
    }
}