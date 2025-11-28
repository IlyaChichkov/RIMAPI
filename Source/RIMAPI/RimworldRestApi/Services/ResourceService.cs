using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using Verse;

namespace RIMAPI.Services
{
    public class ResourceService : IResourceService
    {
        public ResourceService() { }

        public ApiResult<Dictionary<string, List<ThingDto>>> GetAllStoredResources(int mapId)
        {
            Map map = MapHelper.GetMapByID(mapId);
            var items = ResourcesHelper.GetItemsFromStorageLocations(map);
            LogApi.Info("Items: " + items.Count);
            var result = ResourcesHelper.GetStoredItemsByCategory(items);
            return ApiResult<Dictionary<string, List<ThingDto>>>.Ok(result);
        }

        public ApiResult<List<ThingDto>> GetAllStoredResourcesByCategory(
            int mapId,
            string categoryDef
        )
        {
            Map map = MapHelper.GetMapByID(mapId);
            var items = ResourcesHelper.GetItemsFromStorageLocations(map);
            var result = ResourcesHelper.GetStoredItemsListByCategory(items, categoryDef);
            return ApiResult<List<ThingDto>>.Ok(result);
        }

        public ApiResult<ResourcesSummaryDto> GetResourcesSummary(int mapId)
        {
            Map map = MapHelper.GetMapByID(mapId);
            var result = ResourcesHelper.GenerateResourcesSummary(map);
            return ApiResult<ResourcesSummaryDto>.Ok(result);
        }

        public ApiResult<StoragesSummaryDto> GetStoragesSummary(int mapId)
        {
            Map map = MapHelper.GetMapByID(mapId);
            var result = ResourcesHelper.StoragesSummary(map);
            return ApiResult<StoragesSummaryDto>.Ok(result);
        }
    }
}
