#region file_header

// QuickVideoInfo - YTII.ModelFactory - StreamableVideoModel.cs
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

namespace YTII.ModelFactory.Models
{
    public class StreamableVideoModel : IVideoModel
    {
        public bool IsErrorModel { get; internal set; } = false;

        public float? VideoDurationSeconds { get; set; }

        public string VideoId { get; set; }

        public string VideoFullUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; } = string.Empty;

        public string DefaultThumbnailUrl { get; set; }

        public TimeSpan? VideoDuration
        {
            get => TimeSpan.FromSeconds((int)(VideoDurationSeconds ?? 0));
            set => VideoDurationSeconds = (float?)(value ?? TimeSpan.FromSeconds(0)).TotalSeconds;
        }

        public string VideoDurationString => VideoDuration.PrettyTimeSpanString();
    }
}