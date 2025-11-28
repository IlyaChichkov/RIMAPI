using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RIMAPI.Core
{
    public static class HttpListenerRequestExtensions
    {
        /// <summary>
        /// Read and deserialize the request body as JSON
        /// </summary>
        public static async Task<T> ReadBodyAsync<T>(this HttpListenerRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!request.HasEntityBody)
                return default(T);

            try
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    var json = await reader.ReadToEndAsync();
                    if (string.IsNullOrEmpty(json))
                        return default(T);

                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error reading request body: {ex}");
                return default(T);
            }
        }

        /// <summary>
        /// Read the request body as string
        /// </summary>
        public static async Task<string> ReadBodyAsStringAsync(this HttpListenerRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!request.HasEntityBody)
                return string.Empty;

            try
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error reading request body as string: {ex}");
                return string.Empty;
            }
        }
    }
}
