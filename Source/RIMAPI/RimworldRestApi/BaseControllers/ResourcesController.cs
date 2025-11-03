using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Core;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class ResourcesController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public ResourcesController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task GetResourcesSummary(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object resourcesSummary = _gameDataService.GetResourcesSummary(mapId);
                HandleFiltering(context, ref resourcesSummary);
                await ResponseBuilder.Success(context.Response, resourcesSummary);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetStoragesSummary(HttpListenerContext context)
        {
            try
            {
                var mapId = GetMapIdProperty(context);
                object resourcesSummary = _gameDataService.GetStoragesSummary(mapId);
                HandleFiltering(context, ref resourcesSummary);
                await ResponseBuilder.Success(context.Response, resourcesSummary);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task GetResourcesStored(HttpListenerContext context)
        {
            try
            {
                // TODO: add caching
                object resources = null;
                var mapId = GetMapIdProperty(context);
                var categoryDef = GetStringProperty(context, "category", false);
                if (categoryDef != null)
                {
                    resources = _gameDataService.GetAllStoredResourcesByCategory(mapId, categoryDef);
                }
                else
                {
                    resources = _gameDataService.GetAllStoredResources(mapId);
                }
                HandleFiltering(context, ref resources);
                await ResponseBuilder.Success(context.Response, resources);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}