using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Verse;

namespace RIMAPI.Core
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

    // ApiResult classes for standardized responses
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResult<T> Ok(T data) => new ApiResult<T> { Success = true, Data = data };

        public static ApiResult<T> Fail(string error) =>
            new ApiResult<T> { Success = false, Errors = { error } };

        public static ApiResult<T> Partial(T data, IEnumerable<string> warnings) =>
            new ApiResult<T>
            {
                Success = true,
                Data = data,
                Warnings = warnings.ToList(),
            };
    }

    public class ApiResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResult Ok() => new ApiResult { Success = true };

        public static ApiResult Fail(string error) =>
            new ApiResult { Success = false, Errors = { error } };

        public static ApiResult Unimplemented() =>
            new ApiResult { Success = false, Errors = { "Not implemented" } };

        public static ApiResult Partial(IEnumerable<string> warnings) =>
            new ApiResult { Success = true, Warnings = warnings.ToList() };
    }

    public static class ResponseBuilder
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new SnakeCaseContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };

        // Legacy methods for backward compatibility
        public static async Task Success(HttpListenerResponse response, object data)
        {
            await WriteResponse(response, HttpStatusCode.OK, data);
        }

        public static async Task Error(
            HttpListenerResponse response,
            HttpStatusCode statusCode,
            string message
        )
        {
            LogApi.Warning($"API Error {statusCode}: {message}");
            await WriteResponse(response, statusCode, new { error = message });
        }

        // New methods with ApiResult
        public static async Task SendApiResult<T>(
            HttpListenerResponse response,
            ApiResult<T> result
        )
        {
            var statusCode = DetermineStatusCode(result);
            await WriteResponse(response, statusCode, result);
        }

        public static async Task SendApiResult(HttpListenerResponse response, ApiResult result)
        {
            var statusCode = DetermineStatusCode(result);
            await WriteResponse(response, statusCode, result);
        }

        // Convenience methods for common scenarios
        public static async Task SendSuccess<T>(HttpListenerResponse response, T data)
        {
            await SendApiResult(response, ApiResult<T>.Ok(data));
        }

        public static async Task SendSuccess(HttpListenerResponse response)
        {
            await SendApiResult(response, ApiResult.Ok());
        }

        public static async Task SendError(
            HttpListenerResponse response,
            HttpStatusCode statusCode,
            string errorMessage
        )
        {
            var result = ApiResult.Fail(errorMessage);
            await SendApiResult(response, result);
        }

        public static async Task SendValidationError(
            HttpListenerResponse response,
            IEnumerable<string> errors
        )
        {
            var result = new ApiResult { Success = false, Errors = errors.ToList() };
            await SendApiResult(response, result);
        }

        public static async Task SendPartialSuccess<T>(
            HttpListenerResponse response,
            T data,
            IEnumerable<string> warnings
        )
        {
            var result = ApiResult<T>.Partial(data, warnings);
            await SendApiResult(response, result);
        }

        private static HttpStatusCode DetermineStatusCode<T>(ApiResult<T> result)
        {
            if (result.Success)
            {
                if (result.Warnings?.Any() == true)
                    return HttpStatusCode.OK; // 200 with warnings
                return HttpStatusCode.OK; // 200
            }
            else
            {
                if (result.Errors?.Any(e => e.Contains("not found")) == true)
                    return HttpStatusCode.NotFound; // 404
                if (result.Errors?.Any(e => e.Contains("unauthorized")) == true)
                    return HttpStatusCode.Unauthorized; // 401
                if (result.Errors?.Any(e => e.Contains("validation")) == true)
                    return HttpStatusCode.BadRequest; // 400

                return HttpStatusCode.InternalServerError; // 500
            }
        }

        private static HttpStatusCode DetermineStatusCode(ApiResult result)
        {
            if (result.Success)
            {
                if (result.Warnings?.Any() == true)
                    return HttpStatusCode.OK;
                return HttpStatusCode.OK;
            }
            else
            {
                if (result.Errors?.Any(e => e.Contains("not found")) == true)
                    return HttpStatusCode.NotFound;
                if (result.Errors?.Any(e => e.Contains("unauthorized")) == true)
                    return HttpStatusCode.Unauthorized;
                if (result.Errors?.Any(e => e.Contains("validation")) == true)
                    return HttpStatusCode.BadRequest;

                return HttpStatusCode.InternalServerError;
            }
        }

        private static async Task WriteResponse(
            HttpListenerResponse response,
            HttpStatusCode statusCode,
            object data
        )
        {
            try
            {
                // Ensure the response status code is set correctly
                response.StatusCode = (int)statusCode;

                // Set Content-Type header for JSON responses
                response.ContentType = "application/json; charset=utf-8";

                // Serialize the data to a JSON string
                var json = JsonConvert.SerializeObject(data, _jsonSettings);

                // Log the response for debugging (truncate long responses)
                var logJson = json.Length > 1000 ? json.Substring(0, 1000) + "..." : json;
                LogApi.Message($"Response [{statusCode}]: {logJson}", LoggingLevels.DEBUG);

                // Convert the JSON string to bytes
                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;

                // Write the JSON data to the response output stream
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();

                // Log success for debugging purposes
                LogApi.Info($"Response sent - Status: {statusCode}, Length: {buffer.Length} bytes");
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error writing response: {ex.Message}");
                try
                {
                    response.Abort();
                }
                catch
                {
                    // Ignore any errors on response abort
                }
            }
        }

        // Helper method for sending raw JSON (for SSE or special cases)
        public static async Task SendRawJson(
            HttpListenerResponse response,
            string json,
            HttpStatusCode statusCode = HttpStatusCode.OK
        )
        {
            try
            {
                response.StatusCode = (int)statusCode;
                response.ContentType = "application/json; charset=utf-8";

                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error sending raw JSON: {ex.Message}");
                try
                {
                    response.Abort();
                }
                catch { }
            }
        }

        // Helper method for sending plain text
        public static async Task SendPlainText(
            HttpListenerResponse response,
            string text,
            HttpStatusCode statusCode = HttpStatusCode.OK
        )
        {
            try
            {
                response.StatusCode = (int)statusCode;
                response.ContentType = "text/plain; charset=utf-8";

                var buffer = Encoding.UTF8.GetBytes(text);
                response.ContentLength64 = buffer.Length;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error sending plain text: {ex.Message}");
                try
                {
                    response.Abort();
                }
                catch { }
            }
        }
    }

    // Extension methods for easier response building in controllers
    public static class ResponseBuilderExtensions
    {
        public static async Task SendJsonResponse<T>(
            this HttpListenerContext context,
            ApiResult<T> result
        )
        {
            await ResponseBuilder.SendApiResult(context.Response, result);
        }

        public static async Task SendJsonResponse(
            this HttpListenerContext context,
            ApiResult result
        )
        {
            await ResponseBuilder.SendApiResult(context.Response, result);
        }

        public static async Task SendSuccess<T>(this HttpListenerContext context, T data)
        {
            await ResponseBuilder.SendSuccess(context.Response, data);
        }

        public static async Task SendSuccess(this HttpListenerContext context)
        {
            await ResponseBuilder.SendSuccess(context.Response);
        }

        public static async Task SendError(
            this HttpListenerContext context,
            HttpStatusCode statusCode,
            string errorMessage
        )
        {
            await ResponseBuilder.SendError(context.Response, statusCode, errorMessage);
        }

        public static async Task SendValidationError(
            this HttpListenerContext context,
            IEnumerable<string> errors
        )
        {
            await ResponseBuilder.SendValidationError(context.Response, errors);
        }

        public static async Task SendPartialSuccess<T>(
            this HttpListenerContext context,
            T data,
            IEnumerable<string> warnings
        )
        {
            await ResponseBuilder.SendPartialSuccess(context.Response, data, warnings);
        }
    }
}
