#region file_header

// QuickVideoInfo - YTII.ModelFactory - VimeoVideoModel.cs
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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace YTII.ModelFactory.Models
{
    public class VimeoVideoModel : IVideoModel
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("created_time")]
        public DateTime CreatedTime { get; set; }

        [JsonProperty("content_rating")]
        public List<string> ContentRating { get; set; }

        [JsonProperty("pictures")]
        public Pictures Thumbnails { get; set; }

        [JsonProperty("stats")]
        public Stats VideoStats { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonIgnore]
        public bool IsErrorModel { get; set; } = false;

        [JsonProperty("name")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("link")]
        public string VideoFullUrl { get; set; }


        [JsonIgnore]
        public string VideoId
        {
            get => VideoFullUrl.Substring(VideoFullUrl.LastIndexOf(".com/", StringComparison.OrdinalIgnoreCase) + 5).TrimEnd('/');
            set { }
        }

        [JsonIgnore]
        public string DefaultThumbnailUrl
        {
            get
            {
                return Thumbnails.Sizes.OrderBy(p => p.Width)
                                 .SkipWhile((p, i) => i < Thumbnails.Sizes.Count / 2)
                                 .FirstOrDefault()
                                 ?
                                 .Link ?? Thumbnails.Sizes.FirstOrDefault()?.Link;
            }
            set { }
        }

        [JsonIgnore]
        public TimeSpan? VideoDuration
        {
            get => TimeSpan.FromSeconds(Duration);
            set => Duration = (int)(value?.TotalSeconds ?? 0);
        }

        [JsonIgnore]
        public string VideoDurationString
        {
            get
            {
                var videoDurationString = (VideoDuration ?? TimeSpan.FromMinutes(0)).ToString().TrimStart('0', ':');
                switch (videoDurationString.Length)
                {
                    case 0:
                        videoDurationString = "0:00";
                        break;
                    case 1:
                        videoDurationString = "0:0" + videoDurationString;
                        break;
                    case 2:
                        videoDurationString = "0:" + videoDurationString;
                        break;
                    case 3:
                        videoDurationString = "0" + videoDurationString;
                        break;
                }
                return videoDurationString;
            }
        }

        public class Pictures
        {
            [JsonProperty("uri")]
            public string Uri { get; set; }

            [JsonProperty("active")]
            public bool Active { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("sizes")]
            public List<Size> Sizes { get; set; }

            [JsonProperty("resource_key")]
            public string ResourceKey { get; set; }

            public class Size
            {
                [JsonProperty("width")]
                public int Width { get; set; }

                [JsonProperty("height")]
                public int Height { get; set; }

                [JsonProperty("link")]
                public string Link { get; set; }

                [JsonProperty("link_with_play_button")]
                public string LinkWithPlayButton { get; set; }
            }
        }

        public class Stats
        {
            [JsonProperty("plays")]
            public int Plays { get; set; }
        }
    }
}