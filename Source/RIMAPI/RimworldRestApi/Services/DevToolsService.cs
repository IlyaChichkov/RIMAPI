using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using Verse;

namespace RIMAPI.Services
{
    public class DevToolsService : IDevToolsService
    {
        public DevToolsService() { }

        public ApiResult ConsoleAction(string action, string message = null)
        {
            switch (action)
            {
                case "clear":
                    Log.Clear();
                    break;
                case "reset_msg_cnt":
                    Log.ResetMessageCount();
                    break;
                case "message":
                    Log.Message(message);
                    break;
                case "warning":
                    Log.Warning(message);
                    break;
                case "error":
                    Log.Error(message);
                    break;
            }
            return ApiResult.Ok();
        }

        public ApiResult<MaterialsAtlasList> GetMaterialsAtlasList()
        {
            MaterialsAtlasList atlasList = new MaterialsAtlasList
            {
                Materials = new List<string>(),
            };
            try
            {
                foreach (var mat in TextureHelper.GetAtlasDictionaryMaterials())
                {
                    atlasList.Materials.Add(mat.name);
                }
            }
            catch (System.Exception)
            {
                LogApi.Error("Failed to get materials from atlas pool");
            }
            return ApiResult<MaterialsAtlasList>.Ok(atlasList);
        }

        public ApiResult MaterialsAtlasPoolClear()
        {
            TextureHelper.GetAtlasDictionary().Clear();
            TextureHelper.RefreshGraphics();
            return ApiResult.Ok();
        }

        public ApiResult SetStuffColor(StuffColorRequest stuffColor)
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
    }
}
