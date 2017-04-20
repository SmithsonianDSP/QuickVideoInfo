using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using YTII.ModelFactory.Models;

namespace YTII.ModelFactory.Factories
{
    public static class YouTubeVideoFactory
    {
        public static YouTubeVideoModel GetModelFromJson(string jsonPayload)
        {

            JToken result;
            JObject snippet;
            JObject statistics;
            JObject videoDetails;
            try
            {
                JObject response = JsonConvert.DeserializeObject<dynamic>(jsonPayload);

                result = response.Value<JArray>("items").First;

                if (result == null)
                {
                    return GetCannotLoadVideoModel();
                }

                System.Diagnostics.Debug.WriteLine(result.ToString());

                snippet = result.Value<JObject>("snippet");
                statistics = result.Value<JObject>("statistics");
                videoDetails = result.Value<JObject>("contentDetails");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(nameof(GetModelFromJson));
                System.Diagnostics.Debug.WriteLine("\t Exception: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("\tPayload: " + jsonPayload);
                return GetErrorLoadingVideoModel();
            }

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
                System.Diagnostics.Debug.WriteLine(nameof(GetModelFromJson) + "2");
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return GetErrorLoadingVideoModel();
            }
        }

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

        public static YouTubeVideoModel GetErrorLoadingVideoModel()
        {
            return new YouTubeVideoModel
            {
                VideoId = "-1",
                Title = "There was a problem loading video info",
                Description = "There was a problem loading video info",
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


        private static readonly string CantLoadThumbnailBaseUrl = "Jn2grYW";

    }
}
