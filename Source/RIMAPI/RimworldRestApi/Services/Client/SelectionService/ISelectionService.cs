using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface ISelectionService
    {
        ApiResult SelectArea(SelectAreaRequestDto body);
        ApiResult Select(string objectType, int id);
        ApiResult DeselectAll();
    }
}
