using System;
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Models;
using Verse;

namespace RIMAPI.Services
{
    public class ImageService : IImageService
    {
        public ImageService() { }

        public ApiResult<ImageDto> GetItemImage(string name)
        {
            var result = TextureHelper.GetItemImageByName(name);
            return ApiResult<ImageDto>.Ok(result);
        }

        public ApiResult<ImageDto> GetTerrainImage(string name)
        {
            var result = TextureHelper.GetTerrainImageByName(name);
            return ApiResult<ImageDto>.Ok(result);
        }

        public ApiResult SetItemImageByName(ImageUploadRequest imageUpload)
        {
            string imageBase64 = imageUpload.Image;
            const string dataPrefix = "base64,";
            var idx = imageBase64.IndexOf(dataPrefix, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                imageBase64 = imageBase64.Substring(idx + dataPrefix.Length);
            }

            try
            {
                TextureHelper.SetItemImageByName(imageUpload, imageBase64);
            }
            catch (Exception ex)
            {
                ApiResult.Fail($"Failed to process image: {ex.Message}");
                throw;
            }
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
