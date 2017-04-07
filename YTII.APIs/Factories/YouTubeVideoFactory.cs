using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using YTII.APIs.Models;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace YTII.APIs.Factories
{
    public static class YouTubeVideoFactory
    {
        private static string ApiKey { get => Keys.YouTubeApiKey; }

        static readonly private string baseApiUrl = "https://www.googleapis.com/youtube/v3/";

        static readonly private string apiUrlForChannel = baseApiUrl + "search?part=id&maxResults=20&channelId={0}&key=" + ApiKey;
        static readonly private string apiUrlForPlaylist = baseApiUrl + "playlistItems?part=contentDetails&maxResults=20&playlistId={0}&key=" + ApiKey;
        static readonly private string apiUrlForVideosDetails = baseApiUrl + @"videos?part=snippet,statistics,contentDetails&id={0}&key=" + ApiKey;
        static readonly private string apiUrl2ForVideosDetails = baseApiUrl + @"videos?fields=items(id,snippet(title,description,publishedAt,thumbnails),statistics,contentDetails/duration)&part=snippet,statistics,contentDetails&id={0}&key=" + ApiKey;

        //&fields=items(id,snippet(title,description,publishedAt,thumbnails),statistics)&part=snippet,statistics
        public static async Task<YouTubeVideoModel> GetVideoDetailsAsync(string videoId)
        {
            var httpClient = new HttpClient();

            JToken result;
            JObject snippet;
            JObject statistics;
            JObject videoDetails;
            try
            {
                var json = await httpClient.GetStringAsync(string.Format(apiUrl2ForVideosDetails, videoId)).ConfigureAwait(true);

                JObject response = JsonConvert.DeserializeObject<dynamic>(json);

                result = response.Value<JArray>("items").First;
                snippet = result.Value<JObject>("snippet");
                statistics = result.Value<JObject>("statistics");
                videoDetails = result.Value<JObject>("contentDetails");
            }
            catch (Exception ex) { return null; }

            try
            {
                var thumbnails = snippet?.Value<JObject>("thumbnails");

                return new YouTubeVideoModel
                {
                    VideoId = result?.Value<string>("id"),

                    Title = snippet?.Value<string>("title"),
                    Description = snippet?.Value<string>("description"),
                    //ChannelTitle = snippet?.Value<string>("channelTitle"),
                    PublishedAt = snippet?.Value<DateTime>("publishedAt") ?? DateTime.MinValue,

                    MaxResThumbnailUrl = thumbnails?.Value<JObject>("maxres")?.Value<string>("url"),
                    HighThumbnailUrl = thumbnails?.Value<JObject>("high")?.Value<string>("url"),
                    DefaultThumbnailUrl = thumbnails?.Value<JObject>("default")?.Value<string>("url"),
                    MediumThumbnailUrl = thumbnails?.Value<JObject>("medium")?.Value<string>("url"),
                    StandardThumbnailUrl = thumbnails?.Value<JObject>("standard")?.Value<string>("url"),

                    ViewCount = statistics?.Value<int>("viewCount"),
                    LikeCount = statistics?.Value<int>("likeCount"),
                    DislikeCount = statistics?.Value<int>("dislikeCount"),
                    FavoriteCount = statistics?.Value<int>("favoriteCount"),
                    CommentCount = statistics?.Value<int>("commentCount"),

                    VideoDurationISO8601 = videoDetails.Value<string>("duration")
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}
//public static async Task<IEnumerable<Models.YouTubeVideoModel>> GetAllVideoDetailsAsync(IEnumerable<string> videoIds)
//{

//}

//public static async Task<IEnumerable<string>> GetVideoIdsFromChannelAsync()
//{
//    var httpClient = new HttpClient();
//    var videoIds = new List<string>();

//    var json = await httpClient.GetStringAsync(apiUrlForChannel).ConfigureAwait(false);

//    try
//    {
//        JObject response = JsonConvert.DeserializeObject<dynamic>(json);

//        //foreach (var item in response.Value<JArray>("items"))
//        //    videoIds.Add(item.Value<JObject>("id")?.Value<string>("videoId"));

//        return response.Value<JArray>("items")
//                       .Select(i => i.Value<JObject>("id")?.Value<string>("videoId"));
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine(ex.Message);
//        return new string[] { };
//    }
//}

//public static async Task<List<string>> GetVideoIdsFromPlaylistAsync()
//{
//    var httpClient = new HttpClient();
//    var json = await httpClient.GetStringAsync(apiUrlForPlaylist);
//    var videoIds = new List<string>();

//    try
//    {
//        JObject response = JsonConvert.DeserializeObject<dynamic>(json);

//        var items = response.Value<JArray>("items");

//        foreach (var item in items)
//        {
//            videoIds.Add(item.Value<JObject>("contentDetails")?.Value<string>("videoId"));
//        };

//        YoutubeItems = await GetVideosDetailsAsync(videoIds);
//    }
//    catch (Exception exception)
//    {
//    }

//    return videoIds;
//}
