

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using LudeonTK;
using RimworldRestApi.Core;
using RimworldRestApi.Models;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Controllers
{
    public class DevController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public DevController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task ConsoleAction(HttpListenerContext context)
        {
            try
            {
                string action = GetStringProperty(context, "action");
                string message = GetStringProperty(context, "message", false);
                _gameDataService.ConsoleAction(action, message);
                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task SetItemImage(HttpListenerContext context)
        {
            try
            {
                // Read from JSON body instead of query parameters
                var requestData = await ReadJsonBodyAsync<ImageUploadRequest>(context);
                _gameDataService.SetItemImageByName(requestData);

                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task SetStuffColor(HttpListenerContext context)
        {
            try
            {
                // Read from JSON body instead of query parameters
                var requestData = await ReadJsonBodyAsync<StuffColorRequest>(context);
                _gameDataService.SetStuffColor(requestData);

                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task MaterialsAtlasList(HttpListenerContext context)
        {
            try
            {
                object list = _gameDataService.GetMaterialsAtlasList();
                HandleFiltering(context, ref list);
                await ResponseBuilder.Success(context.Response, list);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task MaterialsAtlasPoolClear(HttpListenerContext context)
        {
            try
            {
                _gameDataService.MaterialsAtlasPoolClear();

                var result = new
                {
                    Result = "success"
                };
                await ResponseBuilder.Success(context.Response, result);
            }
            catch (Exception ex)
            {
                await ResponseBuilder.Error(context.Response,
                    HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}