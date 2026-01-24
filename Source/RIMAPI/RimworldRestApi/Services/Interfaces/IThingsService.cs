
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IThingsService
    {
        ApiResult<ItemRecipesDto> GetItemRecipes(string defName);
        ApiResult<ThingSourcesDto> GetItemSources(string defName);
    }
}
