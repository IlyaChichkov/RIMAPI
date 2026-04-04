
using System.Collections.Generic;
using RIMAPI.Core;
using RIMAPI.Models;

namespace RIMAPI.Services
{
    public interface IImageService
    {
        ApiResult<ImageDto> GetItemImage(string name);
        ApiResult<ImageDto> GetTerrainImage(string name);
        ApiResult SetItemImageByName(ImageUploadRequest request);
        ApiResult SetStuffColor(StuffColorRequest request);
    }
}
