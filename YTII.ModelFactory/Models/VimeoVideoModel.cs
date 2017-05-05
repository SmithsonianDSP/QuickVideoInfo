#region file_header

// QuickVideoInfo - YTII.ModelFactory - VimeoVideoModel.cs
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
using System.Linq;
using Newtonsoft.Json;
using YTII.ModelFactory.Properties;

namespace YTII.ModelFactory.Models
{
    [Preserve(AllMembers = true)]
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
            get
            {
                if (IsErrorModel || string.IsNullOrEmpty(VideoFullUrl))
                    return "-1";

                return VideoFullUrl.Substring(VideoFullUrl.LastIndexOf(".com/", StringComparison.OrdinalIgnoreCase) + 5).TrimEnd('/');
            }
            set { }
        }

        [JsonIgnore]
        public string DefaultThumbnailUrl
        {
            get
            {
                var thumb = Thumbnails?.Sizes?
                                       .Where(t => !string.IsNullOrEmpty(t.Link))
                                       .OrderBy(p => p.Width)
                                       .ToArray();

                // Skip While index < max index && index <= midpoint index
                var t1 = thumb?.SkipWhile((p, i) => (i < (thumb.Length - 1)) && i <= (thumb.Length / 2)).FirstOrDefault();
                var t2 = thumb?.FirstOrDefault();
                return t1?.Link ?? t2?.Link ?? FallbackThumbnailUrl;
            }
            set { }
        }

        const string FallbackThumbnailUrl = @"http://i.imgur.com/WsK3BA8.png";


        [JsonIgnore]
        public TimeSpan? VideoDuration
        {
            get => TimeSpan.FromSeconds(Duration);
            set => Duration = (int)(value?.TotalSeconds ?? 0);
        }

        [JsonIgnore]
        public string VideoDurationString => VideoDuration.PrettyTimeSpanString();

        [Preserve(AllMembers = true)]
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

            [Preserve(AllMembers = true)]
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

        [Preserve(AllMembers = true)]
        public class Stats
        {
            [JsonProperty("plays")]
            public int Plays { get; set; }
        }
    }
}