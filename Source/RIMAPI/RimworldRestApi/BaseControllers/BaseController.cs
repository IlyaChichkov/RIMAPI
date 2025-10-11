using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;

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

            var type = obj.GetType();
            var result = new Dictionary<string, object>();

            foreach (var field in fields)
            {
                var property = type.GetProperty(field,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                if (property != null && property.CanRead)
                {
                    result[field] = property.GetValue(obj);
                }
            }

            return result;
        }

        protected string GenerateHash(params object[] values)
        {
            var content = string.Join("|", values);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(content))
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}