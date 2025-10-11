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
            Log.Warning($"RIMAPI: API Error {statusCode}: {message}");
            await WriteResponse(response, statusCode, new { error = message });
        }

        private static async Task WriteResponse(HttpListenerResponse response,
            HttpStatusCode statusCode, object data)
        {
            try
            {
                response.StatusCode = (int)statusCode;
                response.ContentType = "application/json";
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, ETag, If-None-Match");

                var json = JsonConvert.SerializeObject(data);

                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();

                Log.Message($"RIMAPI: Response sent - Status: {statusCode}, Length: {buffer.Length}");
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error writing response: {ex.Message}");
                try
                {
                    response.Abort();
                }
                catch
                {
                    // Ignore abort errors
                }
            }
        }
    }
}