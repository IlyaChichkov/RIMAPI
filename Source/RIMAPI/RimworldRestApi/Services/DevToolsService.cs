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
        public DevToolsService() { }

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
    }
}
