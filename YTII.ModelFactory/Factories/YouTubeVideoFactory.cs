#region file_header

// QuickVideoInfo - YTII.ModelFactory - YouTubeVideoFactory.cs
// 
// Licensed to the Apache Software Foundation (ASF) under one or more contributor license agreements.  
// See the NOTICE file distributed with this work for additional information regarding copyright ownership.  
// The ASF licenses this file to you under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express 
// or implied.  See the License for the specific language governing permissions and limitations under the License.
//  

#endregion

using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YTII.ModelFactory.Models;

namespace YTII.ModelFactory.Factories
{
    public static class YouTubeVideoFactory
    {
        static readonly string CantLoadThumbnailBaseUrl = @"Jn2grYW";

        public static YouTubeVideoModel GetModelFromJson(string jsonPayload)
        {
            JToken result;
            JObject snippet;
            JObject statistics;
            JObject videoDetails;
            try
            {
                JObject response = JsonConvert.DeserializeObject<dynamic>(jsonPayload);

                result = response.Value<JArray>(@"items").First;

                if (result == null)
                    return GetCannotLoadVideoModel();

                Debug.WriteLine(result.ToString());

                snippet = result.Value<JObject>(@"snippet");
                statistics = result.Value<JObject>(@"statistics");
                videoDetails = result.Value<JObject>(@"contentDetails");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(nameof(GetModelFromJson));
                Debug.WriteLine("\t Exception: " + ex.Message);
                Debug.WriteLine("\tPayload: " + jsonPayload);
                return GetErrorLoadingVideoModel();
            }

            try
            {
                var thumbnails = snippet?.Value<JObject>(@"thumbnails");

                return new YouTubeVideoModel
                {
                    VideoId = result?.Value<string>(@"id"),

                    Title = snippet?.Value<string>(@"title"),
                    Description = snippet?.Value<string>(@"description"),
                    PublishedAt = snippet?.Value<DateTime>(@"publishedAt") ?? DateTime.MinValue,

                    MaxResThumbnailUrl = thumbnails?.Value<JObject>(@"maxres")?.Value<string>(@"url"),
                    HighThumbnailUrl = thumbnails?.Value<JObject>(@"high")?.Value<string>(@"url"),
                    DefaultThumbnailUrl = thumbnails?.Value<JObject>(@"default")?.Value<string>(@"url"),
                    MediumThumbnailUrl = thumbnails?.Value<JObject>(@"medium")?.Value<string>(@"url"),
                    StandardThumbnailUrl = thumbnails?.Value<JObject>(@"standard")?.Value<string>(@"url"),

                    ViewCount = statistics?.Value<int>(@"viewCount"),
                    LikeCount = statistics?.Value<int>(@"likeCount"),
                    DislikeCount = statistics?.Value<int>(@"dislikeCount"),
                    FavoriteCount = statistics?.Value<int>(@"favoriteCount"),
                    CommentCount = statistics?.Value<int>(@"commentCount"),

                    VideoDurationISO8601 = videoDetails.Value<string>(@"duration")
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(nameof(GetModelFromJson) + "2");
                Debug.WriteLine(ex.Message);
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

                VideoDurationISO8601 = @"PT0M0S"
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

                VideoDurationISO8601 = @"PT0M0S"
            };
        }
    }
}