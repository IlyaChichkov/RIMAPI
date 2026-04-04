using System;
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Services
{
    public class DevToolsService : IDevToolsService
    {
        private readonly IDocumentationService _documentationService;

        public DevToolsService(IDocumentationService documentationService)
        {
            _documentationService = documentationService;
        }

        public ApiResult ConsoleAction(DebugConsoleRequest body)
        {
            try
            {
                switch (body.Action)
                {
                    case "clear":
                        Log.Clear();
                        break;
                    case "reset_msg_cnt":
                        Log.ResetMessageCount();
                        break;
                    case "message":
                        Log.Message(body.Message);
                        break;
                    case "warning":
                        Log.Warning(body.Message);
                        break;
                    case "error":
                        Log.Error(body.Message);
                        break;
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult<MaterialsAtlasList> GetMaterialsAtlasList()
        {
            try
            {
                return ApiResult<MaterialsAtlasList>.Ok(TextureHelper.GetMaterialsAtlasList());
            }
            catch (Exception ex)
            {
                return ApiResult<MaterialsAtlasList>.Fail(ex.Message);
            }
        }

        public ApiResult MaterialsAtlasPoolClear()
        {
            try
            {
                TextureHelper.MaterialsAtlasPoolClear();
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult SetStuffColor(StuffColorRequest stuffColor)
        {
            try
            {
                var modifiedStuff = DefDatabase<ThingDef>.GetNamed(stuffColor.Name);
                modifiedStuff.stuffProps.color = GameTypesHelper.HexToColor(stuffColor.Hex);

                List<Thing> affectedThings = new List<Thing>();
                foreach (Thing thing in Find.CurrentMap.listerThings.AllThings)
                {
                    if (thing.Stuff == modifiedStuff)
                    {
                        affectedThings.Add(thing);
                    }
                }

                foreach (Thing thing in affectedThings)
                {
                    thing.Notify_ColorChanged();
                }
                return ApiResult.Ok();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        public ApiResult<EndpointListDto> GetEndpoints()
        {
            try
            {
                var docs = _documentationService.GenerateDocumentation();
                var endpoints = new List<EndpointDto>();

                foreach (var section in docs.Sections)
                {
                    foreach (var endpoint in section.Endpoints)
                    {
                        endpoints.Add(new EndpointDto
                        {
                            Method = endpoint.Method,
                            Path = endpoint.Path,
                            Description = endpoint.Description,
                            Category = endpoint.Category,
                            Tags = endpoint.Tags,
                            IsDeprecated = !string.IsNullOrEmpty(endpoint.DeprecationNotice)
                        });
                    }
                }

                return ApiResult<EndpointListDto>.Ok(new EndpointListDto
                {
                    Endpoints = endpoints
                });
            }
            catch (Exception ex)
            {
                return ApiResult<EndpointListDto>.Fail(ex.Message);
            }
        }
    }
}
