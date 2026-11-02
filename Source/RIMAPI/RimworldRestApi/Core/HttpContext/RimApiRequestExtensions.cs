using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RIMAPI.Core;
using RIMAPI.Http;

public static class RimApiRequestExtensions
{
    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        ContractResolver = new SnakeCaseContractResolver(),
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
    };

    /// <summary>
    /// Read and deserialize the request body as JSON
    /// </summary>
    public static async Task<T> ReadBodyAsync<T>(this RimApiRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.HasEntityBody)
            return default(T);

        try
        {
            var json = request.ContentEncoding.GetString(await request.ReadBodyBytesAsync());

            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            LogApi.Message($"Receive body: {json}", LoggingLevels.DEBUG);

            return JsonConvert.DeserializeObject<T>(json, JsonSettings);
        }
        catch (Exception ex)
        {
            LogApi.Error($"Error reading request body at {request.Url}: {ex}");
            return default(T);
        }
    }

    /// <summary>
    /// Read the request body as string
    /// </summary>
    public static async Task<string> ReadBodyAsStringAsync(this RimApiRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.HasEntityBody)
            return string.Empty;

        try
        {
            return request.ContentEncoding.GetString(await request.ReadBodyBytesAsync());
        }
        catch (Exception ex)
        {
            LogApi.Error($"Error reading request body as string at {request.Url}: {ex}");
            return string.Empty;
        }
    }
}