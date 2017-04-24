/* ABOUT:
 *      
 * Instead of embedding the YouTube API key directly into the app and having the app request the information from
 * the YouTube API directly, an Azure Function App instead acts as a proxy/cache server for these requests. 
 * 
 * This helps protect the YouTube API Key from being exposed either through an APK breakdown or by HTTP packet
 * inspection, as well as also reducing the number of requests sent to the YouTube API (due to caching) and Azure
 * will also automatically scale to match increases/spikes in requests. 
 * (See also: https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview)
 *    
 *    
 * Requests are sent to the Azure Function App which then checks the recent videos cache (100 items max) and either 
 * returns the result from the cache or it will request it from the YouTube API, add that result to the cache, and
 * then return the result to the requestor. 
 *
 * From my testing, the response speed using the Azure Function Apps proxy is comparable to requesting directly from
 * the YouTube API; when the video requested is *not* cached, it is roughly 75ms slower than sending the request 
 * directly to the YouTube API. when it *is* cached, it is usually between 50-100ms faster. 
 * 
 * 
 * I didn't put a lot of time into adding code comments/documentations, but it should be fairly straightforward.
 * 
 */



using System.Net;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;


internal static TraceWriter appLog;

/// <summary>
/// TODO: Put your YouTube API Key Here
/// </summary>
static readonly string ApiKey = @"YOUR_YOUTUBE_API_KEY_HERE";

/// <summary>
/// TODO: List the SHA1 thumbprints for which requests will be accepted from
/// </summary>
static readonly string[] AuthorizedThumbs = new string[]
{
    @"DEBUG_KEYSTORE_SHA1_THUMBPRINT",
    @"RELEASE_KEYSTORE_SHA1_THUMBPRINT"
};



/// <summary>
/// 
/// </summary>
/// <param name="req"></param>
/// <param name="log"></param>
/// <returns></returns>
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    appLog = log;

    IEnumerable<KeyValuePair<string, string>> parameters = req.GetQueryNameValuePairs().ToArray();
    var videoId = parameters.FirstOrDefault(i => i.Key.Equals("videoid")).Value;
    var thumbprint = parameters.FirstOrDefault(i => i.Key.Equals("thumbprint")).Value;

    log.Info($"Request received for: {videoId}");
    log.Verbose("");
    log.Verbose($"Cache Status:");
    log.Verbose($"\tCached Items: {ResultCache.Count} / {MaxCacheSize}");
    log.Verbose($"\t\tHits: {cacheHits}\t\tMisses: {cacheMisses}");
    log.Verbose("---");

    if (!AuthorizedThumbs.Contains(thumbprint))
    {
        log.Error("ERROR: Invalid Thumbprint");
        return req.CreateResponse(HttpStatusCode.Unauthorized);
    }
    if (videoId == null || videoId.Length == 0)
    {
        log.Error("ERROR: Video ID is Required");
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }
    if (IsItemCached(videoId))
    {
        try
        {
            var result = GetItemFromCache(videoId);
            var payload = new StringContent(result, Encoding.UTF8, "application/json");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Content = payload;
            return response;
        }
        catch (Exception ex)
        {
            log.Error(ex.Message);
            return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    try
    {
        var httpClient = GetHttpClientInstance();

        var requestUrl = string.Format(videoDetailsQuery, videoId);
        var result = await httpClient.GetStringAsync(requestUrl);
        var payload = new StringContent(result, Encoding.UTF8, "application/json");

        AddItemToCache(videoId, result);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Content = payload;
        return response;
    }
    catch (Exception ex)
    {
        log.Error(ex.Message);
        return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
    }
}

#region Item Caches

const int MaxCacheSize = 100;

private static ConcurrentDictionary<string, string> _resultCache;
private static ConcurrentDictionary<string, string> ResultCache
{
    get { return _resultCache ?? (_resultCache = new ConcurrentDictionary<string, string>()); }
    set { _resultCache = value; }
}
private static ConcurrentQueue<string> idCache = new ConcurrentQueue<string>();

internal static void AddItemToCache(string videoId, string item)
{
    appLog?.Info($"Adding video item to cache: {videoId}");

    idCache.Enqueue(videoId);

    while (idCache.Count > MaxCacheSize)
    {
        string idToCull;
        if (idCache.TryDequeue(out idToCull))
        {
            string outval;
            ResultCache.TryRemove(idToCull, out outval);
            appLog?.Info($"Dequeuing excess cache item: {idToCull}");
        }
    }
    ResultCache.TryAdd(videoId, item);
    cacheMisses++;
    appLog?.Info($"Total Cache Size: \t {ResultCache.Count()}");
}

internal static string GetItemFromCache(string videoId)
{
    cacheHits++;
    appLog?.Info($"Returning item from cache: {videoId}");
    var newQueue = new string[100];
    idCache.CopyTo(newQueue, 0);
    idCache = new ConcurrentQueue<string>(newQueue.Except(new string[] { videoId }));
    idCache.Enqueue(videoId);

    return ResultCache[videoId];
}


internal static bool IsItemCached(string videoId)
{
    return ResultCache.ContainsKey(videoId);
}

static int cacheHits = 0;
static int cacheMisses = 0;

#endregion

static readonly string mediaType = "application/json";

static readonly string baseApiUrl = "https://www.googleapis.com/youtube/v3/";
static readonly string videoDetailsQuery = baseApiUrl + @"videos?fields=items(id,snippet(title,description,publishedAt,thumbnails),statistics,contentDetails/duration)&part=snippet,statistics,contentDetails&id={0}&key=" + ApiKey;



// Note: Sharing HttpClient instances as per recommended in the Microsoft documents, below: 
// https://github.com/mspnp/performance-optimization/blob/master/ImproperInstantiation/docs/ImproperInstantiation.md
static HttpClient _httpClient;
static HttpClient GetHttpClientInstance() => _httpClient ?? (_httpClient = new HttpClient());



