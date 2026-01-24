using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class ThingController
    {
        private readonly IResourceService _resourcesService;
        private readonly IThingsService _thingsService;

        public ThingController(IResourceService resourcesService, IThingsService thingsService)
        {
            _resourcesService = resourcesService;
            _thingsService = thingsService;
        }

        [Get("/api/v1/resources/summary")]
        public async Task GetResourcesSummary(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _resourcesService.GetResourcesSummary(mapId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/resources/stored")]
        public async Task GetResourcesStored(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var categoryDef = RequestParser.GetStringParameter(context, "category", false);

            if (string.IsNullOrEmpty(categoryDef))
            {
                var result = _resourcesService.GetAllStoredResources(mapId);
                await context.SendJsonResponse(result);
            }
            else
            {
                var result = _resourcesService.GetAllStoredResourcesByCategory(mapId, categoryDef);
                await context.SendJsonResponse(result);
            }
        }

        [Get("/api/v1/resources/storages/summary")]
        public async Task GetStoragesSummary(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var result = _resourcesService.GetStoragesSummary(mapId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/item/spawn")]
        public async Task SpawnItem(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SpawnItemRequestDto>();
            var result = _resourcesService.SpawnItem(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/item/recipes")]
        public async Task GetItemRecipe(HttpListenerContext context)
        {
            var itemDef = RequestParser.GetStringParameter(context, "def_name");
            var result = _thingsService.GetItemRecipes(itemDef);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/item/sources")]
        public async Task GetItemSource(HttpListenerContext context)
        {
            var itemDef = RequestParser.GetStringParameter(context, "def_name");
            var result = _thingsService.GetItemSources(itemDef);
            await context.SendJsonResponse(result);
        }
    }
}
