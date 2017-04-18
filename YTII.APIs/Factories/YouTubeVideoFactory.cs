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

        static public bool AddApiAuthHeaders = false;
        static public string ApiAuthPackageName { internal get; set; }
        static public string ApiAuthSHA1 { internal get; set; }


        private static void AddAuthHeaders(ref HttpClient httpClient)
        {
            var packageAsReferer = new Uri("http://youtube.com/" + ApiAuthSHA1, UriKind.RelativeOrAbsolute);
            httpClient.DefaultRequestHeaders.Referrer = packageAsReferer;
        }

        //&fields=items(id,snippet(title,description,publishedAt,thumbnails),statistics)&part=snippet,statistics
        public static async Task<YouTubeVideoModel> GetVideoDetailsAsync(string videoId)
        {
            var httpClient = new HttpClient();

            if (AddApiAuthHeaders)
                AddAuthHeaders(ref httpClient);

            JToken result;
            JObject snippet;
            JObject statistics;
            JObject videoDetails;
            try
            {
                var json = await httpClient.GetStringAsync(string.Format(apiUrl2ForVideosDetails, videoId)).ConfigureAwait(true);

                JObject response = JsonConvert.DeserializeObject<dynamic>(json);

                result = response.Value<JArray>("items").First;

                if (result == null)
                {
                    return GetCannotLoadVideoModel(videoId);
                }

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

        public static async Task<YouTubeVideoModel> TestCacheServer(string videoId)
        {
            var thumb = @"7A:EE:BB:22:99:63:71:4B:78:64:BB:E3:AA:88:41:54:77:8C:02:50";
            var functionKey = @"viD4bihaMC3MaybDei4BOdJ2iBOe/VX5/JCKaMdKDfc0Nfmf1xzBPg==";
            var apiUrl = @"https://quickvideoapi.azurewebsites.net/api/HttpTriggerCSharp1?code={0}&videoid={1}&thumbprint={2}";

            var badVideoId = "2srCxPDOyi8";
            var goodVideoId = "X7u1aMN-e64";

            var client = new HttpClient();

            var requestString = string.Format(apiUrl, functionKey, videoId, thumb);
            var json = await client.GetStringAsync(requestString).ConfigureAwait(true);

            JObject response = JsonConvert.DeserializeObject<dynamic>(json);

            JToken result;
            JObject snippet;
            JObject statistics;
            JObject videoDetails;

            result = response.Value<JArray>("items").First;

            if (result == null)
            {
                return GetCannotLoadVideoModel(videoId);
            }

            snippet = result.Value<JObject>("snippet");
            statistics = result.Value<JObject>("statistics");
            videoDetails = result.Value<JObject>("contentDetails");

            try
            {
                var thumbnails = snippet?.Value<JObject>("thumbnails");

                return new YouTubeVideoModel
                {
                    VideoId = result?.Value<string>("id"),

                    Title = snippet?.Value<string>("title"),
                    Description = snippet?.Value<string>("description"),
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



        private static string CantLoadThumbnailBaseUrl = "Jn2grYW";
        public static YouTubeVideoModel GetCannotLoadVideoModel(string videoId = "-1")
        {
            return new YouTubeVideoModel
            {
                VideoId = videoId,
                Title = "This video is no longer available",
                Description = "This video is no longer available",
                PublishedAt = DateTime.Today,

                MaxResThumbnailUrl = $"http://i.imgur.com/{CantLoadThumbnailBaseUrl}.png",
                StandardThumbnailUrl = $"http://i.imgur.com/{CantLoadThumbnailBaseUrl}.png",
                HighThumbnailUrl = $"http://i.imgur.com/{CantLoadThumbnailBaseUrl}.png",
                MediumThumbnailUrl = $"http://i.imgur.com/{CantLoadThumbnailBaseUrl}.png",
                DefaultThumbnailUrl = $"http://i.imgur.com/{CantLoadThumbnailBaseUrl}.png",

                ViewCount = 0,
                LikeCount = 0,
                DislikeCount = 0,
                FavoriteCount = 0,
                CommentCount = 0,

                VideoDurationISO8601 = "PT0M0S"
            };
        }

    }
}
