#region file_header

// QuickVideoInfo - YTII.ModelFactory - YouTubeVideoModel.cs
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
using System.Xml;

namespace YTII.ModelFactory.Models
{
    public class YouTubeVideoModel : IVideoModel
    {
        public string ChannelTitle { get; set; }

        public string VideoDurationISO8601
        {
            set
            {
                try
                {
                    VideoDuration = XmlConvert.ToTimeSpan(value);
                }
                catch
                {
                }
            }
        }

        public string MediumThumbnailUrl { get; set; }

        public string HighThumbnailUrl { get; set; }

        public string StandardThumbnailUrl { get; set; }

        public string MaxResThumbnailUrl { get; set; }

        public DateTime PublishedAt { get; set; }

        public int? ViewCount { get; set; }

        public string ViewCountString => ViewCount.PrettyNumberString();

        public int? LikeCount { get; set; }
        public string LikeCountString => LikeCount.PrettyNumberString();

        public int? DislikeCount { get; set; }

        public string DislikeCountString => DislikeCount.PrettyNumberString();

        public int? FavoriteCount { get; set; }

        public int? CommentCount { get; set; }

        public IEnumerable<string> Tags { get; internal set; } = new List<string>();

        #region Interface Implementations

        public string VideoId { get; set; }

        public string VideoFullUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public TimeSpan? VideoDuration { get; set; }

        public string VideoDurationString => VideoDuration.PrettyTimeSpanString();

        public string DefaultThumbnailUrl { get; set; }

        #endregion
    }
}