using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Verse;

namespace RimworldRestApi.Core
{
    public static class ResponseBuilder
    {
        public static async Task Success(HttpListenerResponse response, object data)
        {
            await WriteResponse(response, HttpStatusCode.OK, data);
        }

        public static async Task Error(HttpListenerResponse response,
            HttpStatusCode statusCode, string message)
        {
            await WriteResponse(response, statusCode, new { error = message });
        }

        private static async Task WriteResponse(HttpListenerResponse response,
            HttpStatusCode statusCode, object data)
        {
            try
            {
                response.StatusCode = (int)statusCode;
                response.ContentType = "application/json";

                var json = JsonConvert.SerializeObject(data);

                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                Log.Error($"Error writing response: {ex.Message}");
                response.Abort();
            }
        }
    }
}