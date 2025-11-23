using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Verse;

using RimworldRestApi.Core;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RimworldRestApi.Controllers
{
    public abstract class BaseController
    {
        protected async Task HandleETagCaching<T>(HttpListenerContext context,
            T data, Func<T, string> hashGenerator) where T : class
        {
            var request = context.Request;
            var response = context.Response;

            // Generate ETag
            var dataHash = hashGenerator(data);
            var etag = $"\"{dataHash}\"";

            // Check If-None-Match
            var ifNoneMatch = request.Headers["If-None-Match"];
            if (ifNoneMatch == etag)
            {
                response.StatusCode = (int)HttpStatusCode.NotModified;
                response.Close();
                return;
            }

            // Add ETag to response
            response.Headers.Add("ETag", etag);
            response.Headers.Add("Cache-Control", "no-cache");

            // Apply fields filtering
            var fieldsParam = request.QueryString["fields"];
            var filteredData = ApplyFieldsFilter(data, fieldsParam);

            await ResponseBuilder.Success(response, filteredData);
        }

        protected void HandleFiltering(HttpListenerContext context, ref object data)
        {
            var fieldsParam = context.Request.QueryString["fields"];
            var filterParam = context.Request.QueryString["filter"];

            data = ApplyFieldsFilter(data, fieldsParam);
            data = ApplyArrayFilter(data, filterParam);
        }

        private object ApplyArrayFilter(object data, string filter)
        {
            if (string.IsNullOrEmpty(filter) || data == null)
                return data;

            try
            {
                // Parse filter expression (e.g., "type=Building_TrapExplosive")
                var filterParts = filter.Split('=');
                if (filterParts.Length != 2)
                {
                    DebugLogging.Warning($"Invalid filter format: {filter}. Expected 'field=value'");
                    return data;
                }

                var fieldName = filterParts[0].Trim();
                var expectedValue = filterParts[1].Trim();

                return FilterArrayByCondition(data, fieldName, expectedValue);
            }
            catch (Exception ex)
            {
                DebugLogging.Warning($"Error applying filter {filter}: {ex.Message}");
                return data;
            }
        }

        private object FilterArrayByCondition(object obj, string fieldName, object expectedValue)
        {
            if (obj == null) return null;

            // Handle arrays/lists
            if (obj is System.Collections.IEnumerable enumerable && obj.GetType() != typeof(string))
            {
                var resultList = new List<object>();
                foreach (var item in enumerable)
                {
                    if (item == null) continue;

                    // Get the field value from the item
                    var fieldValue = GetFieldValue(item, fieldName);

                    // Compare values (supporting string comparison with case insensitivity)
                    if (ValuesMatch(fieldValue, expectedValue))
                    {
                        resultList.Add(item);
                    }
                }

                // Return the filtered list
                return resultList;
            }

            // For single objects, return as-is if it matches the filter
            var singleFieldValue = GetFieldValue(obj, fieldName);
            return ValuesMatch(singleFieldValue, expectedValue) ? obj : null;
        }

        private object GetFieldValue(object obj, string fieldPath)
        {
            if (obj == null) return null;

            // Handle nested properties (e.g., "position.x")
            if (fieldPath.Contains('.'))
            {
                return GetNestedPropertyValue(obj, fieldPath);
            }

            // Handle top-level properties
            var property = GetPropertyInfo(obj.GetType(), fieldPath);
            if (property != null && property.CanRead)
            {
                try
                {
                    return property.GetValue(obj);
                }
                catch (Exception ex)
                {
                    DebugLogging.Warning($"Error getting property {fieldPath}: {ex.Message}");
                }
            }

            // Try fields if properties don't work
            var field = obj.GetType().GetField(fieldPath,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(obj);
            }

            DebugLogging.Warning($"Field '{fieldPath}' not found on type {obj.GetType().Name}");
            return null;
        }

        private bool ValuesMatch(object actualValue, object expectedValue)
        {
            if (actualValue == null && expectedValue == null) return true;
            if (actualValue == null || expectedValue == null) return false;

            // Handle string comparison (case insensitive)
            if (actualValue is string actualStr && expectedValue is string expectedStr)
            {
                return string.Equals(actualStr, expectedStr, StringComparison.OrdinalIgnoreCase);
            }

            // Handle other types
            return actualValue.Equals(expectedValue);
        }

        private object ApplyFieldsFilter(object data, string fields)
        {
            if (string.IsNullOrEmpty(fields) || data == null)
                return data;

            var fieldList = fields.Split(',')
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToList();

            return FilterObjectByFields(data, fieldList);
        }

        private object FilterObjectByFields(object obj, List<string> fields)
        {
            if (obj == null) return null;

            // Handle arrays/lists - THIS IS THE KEY FIX
            if (obj is System.Collections.IEnumerable enumerable && obj.GetType() != typeof(string))
            {
                var resultList = new List<object>();
                foreach (var item in enumerable)
                {
                    // Filter each individual item in the collection
                    var filteredItem = FilterSingleObjectByFields(item, fields);
                    if (filteredItem != null)
                    {
                        resultList.Add(filteredItem);
                    }
                }
                return resultList;
            }

            // Handle single object
            return FilterSingleObjectByFields(obj, fields);
        }

        private object FilterSingleObjectByFields(object obj, List<string> fields)
        {
            if (obj == null) return null;

            var result = new Dictionary<string, object>();
            var objType = obj.GetType();

            foreach (var field in fields)
            {
                // Handle nested properties (e.g., "position.x")
                if (field.Contains('.'))
                {
                    var value = GetNestedPropertyValue(obj, field);
                    if (value != null)
                    {
                        SetNestedProperty(result, field, value);
                    }
                }
                else
                {
                    // Handle top-level properties on the actual object type
                    var property = GetPropertyInfo(objType, field);
                    if (property != null && property.CanRead)
                    {
                        try
                        {
                            var value = property.GetValue(obj);
                            result[field] = value;
                        }
                        catch (Exception ex)
                        {
                            DebugLogging.Warning($"Error getting property {field}: {ex.Message}");
                        }
                    }
                    else
                    {
                        DebugLogging.Warning($"Property '{field}' not found or not readable on type {objType.Name}");
                    }
                }
            }

            return result.Count > 0 ? result : null;
        }

        private PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            return type.GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }

        private object GetNestedPropertyValue(object obj, string propertyPath)
        {
            var currentObj = obj;
            var parts = propertyPath.Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                if (currentObj == null) return null;

                var currentType = currentObj.GetType();
                var property = GetPropertyInfo(currentType, parts[i]);
                if (property == null)
                {
                    DebugLogging.Warning($"Nested property '{parts[i]}' not found in path '{propertyPath}'");
                    return null;
                }

                currentObj = property.GetValue(currentObj);

                // If this is the last part, return the value
                if (i == parts.Length - 1)
                    return currentObj;
            }

            return null;
        }

        private void SetNestedProperty(Dictionary<string, object> dict, string propertyPath, object value)
        {
            var current = dict;
            var parts = propertyPath.Split('.');

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.ContainsKey(parts[i]) ||
                    !(current[parts[i]] is Dictionary<string, object>))
                {
                    current[parts[i]] = new Dictionary<string, object>();
                }
                current = current[parts[i]] as Dictionary<string, object>;
            }

            current[parts[parts.Length - 1]] = value;
        }

        protected string GenerateHash(params object[] values)
        {
            var content = string.Join("|", values);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(content))
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        protected string GenerateObjectHash<T>(T obj) where T : class
        {
            if (obj == null) return "null";

            string json = JsonConvert.SerializeObject(obj);
            return GenerateHash(json);
        }

        protected int GetMapIdProperty(HttpListenerContext context)
        {
            return GetIntProperty(context, "map_id");
        }

        protected int GetIntProperty(HttpListenerContext context, string searchName = "id")
        {
            string idStr = context.Request.QueryString[searchName];
            if (string.IsNullOrEmpty(idStr))
            {
                throw new Exception($"Missing '{searchName}' parameter");
            }

            if (!int.TryParse(idStr, out int id))
            {
                throw new Exception($"Invalid '{searchName}' format");
            }

            return id;
        }

        protected string GetStringProperty(HttpListenerContext context, string searchName = "id", bool required = true)
        {
            string idStr = context.Request.QueryString[searchName];
            if (required && string.IsNullOrEmpty(idStr))
            {
                throw new Exception($"Missing '{searchName}' parameter");
            }

            return idStr;
        }

        /// <summary>
        /// Read and deserialize the JSON request body to T.
        /// - Validates Content-Type (must contain "application/json" unless allowNonJson is true)
        /// - Enforces a size limit (default 1 MB)
        /// - Uses request.ContentEncoding or UTF8
        /// - Throws ArgumentException with clear messages on bad input
        /// </summary>
        /// <typeparam name="T">Target type (e.g., DTO, List&lt;DTO&gt;, JToken)</typeparam>
        /// <param name="context">HttpListenerContext</param>
        /// <param name="required">If true, throws when body is missing/empty</param>
        /// <param name="maxBytes">Max body size in bytes (default 1 MiB)</param>
        /// <param name="allowNonJson">Allow non-JSON content types</param>
        protected async Task<T> ReadJsonBodyAsync<T>(
            HttpListenerContext context,
            bool required = true,
            long maxBytes = 1_048_576,
            bool allowNonJson = false)
        {
            if (context?.Request == null)
                throw new ArgumentException("Invalid HTTP context.");

            var request = context.Request;

            if (!request.HasEntityBody)
            {
                if (required)
                    throw new ArgumentException("Missing request body.");
                return default(T);
            }

            // Content-Type check
            var contentType = request.ContentType ?? string.Empty;
            if (!allowNonJson && contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) < 0)
                throw new ArgumentException($"Unsupported Content-Type '{contentType}'. Expected 'application/json'.");

            // Size checks
            if (request.ContentLength64 > 0 && request.ContentLength64 > maxBytes)
                throw new ArgumentException($"Request body exceeds the limit of {maxBytes} bytes.");

            using (var input = request.InputStream)
            {
                string body;

                // If ContentLength is known, read exactly that much (and it’s within limit).
                if (request.ContentLength64 >= 0)
                {
                    var enc = request.ContentEncoding ?? Encoding.UTF8;
                    using (var sr = new StreamReader(input, enc, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false))
                    {
                        body = await sr.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    // Unknown length: copy with a hard cap to prevent DoS via large body.
                    using (var ms = new MemoryStream())
                    {
                        await CopyToWithLimitAsync(input, ms, maxBytes + 1).ConfigureAwait(false);
                        if (ms.Length > maxBytes)
                            throw new ArgumentException($"Request body exceeds the limit of {maxBytes} bytes.");
                        body = (request.ContentEncoding ?? Encoding.UTF8).GetString(ms.ToArray());
                    }
                }

                if (string.IsNullOrWhiteSpace(body))
                {
                    if (required)
                        throw new ArgumentException("Request body is empty.");
                    return default(T);
                }

                try
                {
                    DebugLogging.Message($"ReadJsonBodyAsync: {body}", LoggingLevels.DEBUG);

                    // If T is a JToken (JObject/JArray), let JSON.NET parse it dynamically.
                    if (typeof(T) == typeof(JToken) || typeof(T) == typeof(JObject) || typeof(T) == typeof(JArray))
                    {
                        var token = JToken.Parse(body);
                        return token.ToObject<T>();
                    }

                    // Otherwise, deserialize to the requested type.
                    var obj = JsonConvert.DeserializeObject<T>(body);
                    if (obj == null && required)
                        throw new ArgumentException("Invalid or empty JSON payload.");
                    return obj;
                }
                catch (JsonReaderException jre)
                {
                    throw new ArgumentException($"Malformed JSON: {jre.Message}");
                }
                catch (JsonSerializationException jse)
                {
                    throw new ArgumentException($"JSON does not match expected schema: {jse.Message}");
                }
            }
        }

        /// <summary>
        /// Convenience wrapper when you specifically expect a JSON array.
        /// Provides a clearer error if the root is not an array.
        /// </summary>
        protected async Task<List<TItem>> ReadJsonArrayBodyAsync<TItem>(
            HttpListenerContext context,
            bool required = true,
            long maxBytes = 1_048_576)
        {
            var token = await ReadJsonBodyAsync<JToken>(context, required, maxBytes).ConfigureAwait(false);
            if (token == null)
                return null;

            if (token.Type != JTokenType.Array)
                throw new ArgumentException("Expected request body to be a JSON array.");

            try
            {
                return token.ToObject<List<TItem>>();
            }
            catch (JsonException jex)
            {
                throw new ArgumentException($"JSON array items could not be deserialized: {jex.Message}");
            }
        }

        /// <summary>
        /// Non-throwing variant. Returns (success, value, errorMessage).
        /// </summary>
        protected async Task<(bool ok, T value, string error)> TryReadJsonBodyAsync<T>(
            HttpListenerContext context,
            long maxBytes = 1_048_576)
        {
            try
            {
                var val = await ReadJsonBodyAsync<T>(context, required: true, maxBytes: maxBytes).ConfigureAwait(false);
                return (true, val, null);
            }
            catch (Exception ex)
            {
                return (false, default(T), ex.Message);
            }
        }

        private static async Task CopyToWithLimitAsync(Stream source, Stream destination, long maxBytes)
        {
            byte[] buffer = new byte[16 * 1024];
            long total = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                total += read;
                if (total > maxBytes)
                {
                    // Write what we have plus one extra chunk detection; caller will check length.
                    await destination.WriteAsync(buffer, 0, read).ConfigureAwait(false);
                    break;
                }
                await destination.WriteAsync(buffer, 0, read).ConfigureAwait(false);
            }
            await destination.FlushAsync().ConfigureAwait(false);
        }
    }
}