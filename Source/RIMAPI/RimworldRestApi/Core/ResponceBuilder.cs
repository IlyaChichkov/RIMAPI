using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Verse;

namespace RimworldRestApi.Core
{
    public class SnakeCaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return ConvertToSnakeCase(propertyName);
        }

        private string ConvertToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder();
            result.Append(char.ToLower(input[0]));

            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    result.Append('_');
                    result.Append(char.ToLower(input[i]));
                }
                else
                {
                    result.Append(input[i]);
                }
            }

            return result.ToString();
        }
    }

    public static class ResponseBuilder
    {
        public static async Task Success(HttpListenerResponse response, object data)
        {
            await WriteResponse(response, HttpStatusCode.OK, data);
        }

        public static async Task Error(HttpListenerResponse response,
            HttpStatusCode statusCode, string message)
        {
            DebugLogging.Warning($"API Error {statusCode}: {message}");
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

                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new SnakeCaseContractResolver(),
                };

                var json = JsonConvert.SerializeObject(data, settings);

                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();

                DebugLogging.Info($"Response sent - Status: {statusCode}, Length: {buffer.Length}");
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Error writing response: {ex.Message}");
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