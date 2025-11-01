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
                    Log.Warning($"[RIMAPI] Invalid filter format: {filter}. Expected 'field=value'");
                    return data;
                }

                var fieldName = filterParts[0].Trim();
                var expectedValue = filterParts[1].Trim();

                return FilterArrayByCondition(data, fieldName, expectedValue);
            }
            catch (Exception ex)
            {
                Log.Warning($"[RIMAPI] Error applying filter {filter}: {ex.Message}");
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
                    Log.Warning($"[RIMAPI] Error getting property {fieldPath}: {ex.Message}");
                }
            }

            // Try fields if properties don't work
            var field = obj.GetType().GetField(fieldPath,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(obj);
            }

            Log.Warning($"[RIMAPI] Field '{fieldPath}' not found on type {obj.GetType().Name}");
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
                            Log.Warning($"[RIMAPI] Error getting property {field}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Log.Warning($"[RIMAPI] Property '{field}' not found or not readable on type {objType.Name}");
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
                    Log.Warning($"[RIMAPI] Nested property '{parts[i]}' not found in path '{propertyPath}'");
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

        protected string GetStringProperty(HttpListenerContext context, string searchName = "id")
        {
            string idStr = context.Request.QueryString[searchName];
            if (string.IsNullOrEmpty(idStr))
            {
                throw new Exception($"Missing '{searchName}' parameter");
            }

            return idStr;
        }
    }
}