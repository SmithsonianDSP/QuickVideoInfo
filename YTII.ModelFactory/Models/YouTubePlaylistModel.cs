#region file_header

// QuickVideoInfo - YTII.ModelFactory - YouTubePlaylistModel.cs
// 
// This file is licensed to you under the Apache License, Version 2.0 (the "License"); you may not use 
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
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YTII.ModelFactory.Models
{
    public class YouTubePlaylistModel
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("snippet")]
        public PlaylistSnippet Snippet { get; set; }

        [JsonProperty("status")]
        public Status PlaylistStatus { get; set; }

        [JsonProperty("contentDetails")]
        public PlaylistDetails ContentDetails { get; set; }

        [JsonProperty("items")]
        public List<PlaylistItem> Items { get; set; }

        public class PlaylistSnippet
        {
            [JsonProperty("publishedAt")]
            public DateTime PublishedAt { get; set; }

            [JsonProperty("channelId")]
            public string ChannelId { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("thumbnails")]
            public Thumbnails Thumbnails { get; set; }

            [JsonProperty("channelTitle")]
            public string ChannelTitle { get; set; }
        }

        public class Thumbnails
        {
            [JsonProperty("default")]
            public Default Thumb0Default { get; set; }

            [JsonProperty("medium")]
            public Medium Thumb1Medium { get; set; }

            [JsonProperty("high")]
            public High Thumb2High { get; set; }

            [JsonProperty("standard")]
            public Standard Thumb3Standard { get; set; }

            [JsonProperty("maxres")]
            public Maxres Thumb4Maxres { get; set; }

            public class Default
            {
                [JsonProperty("url")]
                public string Url { get; set; }

                [JsonProperty("width")]
                public int Width { get; set; }

                [JsonProperty("height")]
                public int Height { get; set; }
            }

            public class Medium
            {
                [JsonProperty("url")]
                public string Url { get; set; }

                [JsonProperty("width")]
                public int Width { get; set; }

                [JsonProperty("height")]
                public int Height { get; set; }
            }

            public class High
            {
                [JsonProperty("url")]
                public string Url { get; set; }

                [JsonProperty("width")]
                public int Width { get; set; }

                [JsonProperty("height")]
                public int Height { get; set; }
            }

            public class Standard
            {
                [JsonProperty("url")]
                public string Url { get; set; }

                [JsonProperty("width")]
                public int Width { get; set; }

                [JsonProperty("height")]
                public int Height { get; set; }
            }

            public class Maxres
            {
                [JsonProperty("url")]
                public string Url { get; set; }

                [JsonProperty("width")]
                public int Width { get; set; }

                [JsonProperty("height")]
                public int Height { get; set; }
            }
        }

        public class Status
        {
            [JsonProperty("privacyStatus")]
            public string PrivacyStatus { get; set; }
        }

        public class PlaylistDetails
        {
            [JsonProperty("itemCount")]
            public int ItemCount { get; set; }
        }

        public class PlaylistItem
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("snippet")]
            public VideoSnippet Snippet { get; set; }

            [JsonProperty("contentDetails")]
            public ContentDetails VideoDetails { get; set; }

            [JsonProperty("status")]
            public Status Status { get; set; }

            public class VideoSnippet
            {
                [JsonProperty("publishedAt")]
                public DateTime PublishedAt { get; set; }

                [JsonProperty("channelId")]
                public string ChannelId { get; set; }

                [JsonProperty("title")]
                public string Title { get; set; }

                [JsonProperty("description")]
                public string Description { get; set; }

                [JsonProperty("thumbnails")]
                public Thumbnails Thumbnails { get; set; }

                [JsonProperty("channelTitle")]
                public string ChannelTitle { get; set; }

                [JsonProperty("playlistId")]
                public string PlaylistId { get; set; }

                [JsonProperty("position")]
                public int Position { get; set; }

                [JsonProperty("resourceId")]
                public ResourceId ResourceId { get; set; }
            }

            public class ResourceId
            {
                [JsonProperty("kind")]
                public string Kind { get; set; }

                [JsonProperty("videoId")]
                public string VideoId { get; set; }
            }

            public class ContentDetails
            {
                [JsonProperty("videoId")]
                public string VideoId { get; set; }

                [JsonProperty("videoPublishedAt")]
                public DateTime VideoPublishedAt { get; set; }
            }
        }
    }
}