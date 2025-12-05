using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class ImageController
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [Get("/api/v1/item/image")]
        [EndpointMetadata("Get item's texture image in base64 format")]
        public async Task GetItemImage(HttpListenerContext context)
        {
            var name = RequestParser.GetStringParameter(context, "name");
            var result = _imageService.GetItemImage(name);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/item/image")]
        [EndpointMetadata("Get item's texture image in base64 format")]
        public async Task SetItemImage(HttpListenerContext context)
        {
            var requestData = await context.Request.ReadBodyAsync<ImageUploadRequest>();
            var result = _imageService.SetItemImageByName(requestData);
            await context.SendJsonResponse(result);
        }
    }
}
