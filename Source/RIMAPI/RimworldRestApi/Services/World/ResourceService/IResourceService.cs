
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IResourceService
    {
        ApiResult<ResourcesSummaryDto> GetResourcesSummary(int mapId);
        ApiResult<StoragesSummaryDto> GetStoragesSummary(int mapId);
        ApiResult<Dictionary<string, List<ThingDto>>> GetAllStoredResources(int mapId);
        ApiResult<List<ThingDto>> GetAllStoredResourcesByCategory(int mapId, string categoryDef);
        ApiResult SpawnItem(SpawnItemRequestDto body);
    }
}
