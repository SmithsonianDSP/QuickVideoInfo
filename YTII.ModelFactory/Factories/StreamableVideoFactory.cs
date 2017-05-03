#region file_header

// QuickVideoInfo - YTII.ModelFactory - StreamableVideoFactory.cs
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YTII.ModelFactory.Models;

namespace YTII.ModelFactory.Factories
{
    public static class StreamableVideoFactory
    {
        static readonly string CantLoadThumbnailBaseUrl = @"Jn2grYW";

        public static StreamableVideoModel GetModelFromJson(string payload)
        {
            try
            {
                JToken result = JsonConvert.DeserializeObject<dynamic>(payload);
                var fileInfo = result.Value<JObject>(@"files")?.First?.First;

                var videoFullUrl = @"https://" + result.Value<string>(@"url");

                return new StreamableVideoModel
                {
                    VideoId = videoFullUrl.Substring(videoFullUrl.LastIndexOf(@".com", StringComparison.OrdinalIgnoreCase) + 5),
                    DefaultThumbnailUrl = @"http:" + result.Value<string>(@"thumbnail_url"),
                    Title = result.Value<string>(@"title"),
                    VideoFullUrl = videoFullUrl,
                    VideoDurationSeconds = fileInfo?.Value<float?>(@"duration")
                };
            }
            catch
            {
                return GetCannotLoadVideoModel();
            }
        }

        public static StreamableVideoModel GetCannotLoadVideoModel(string videoId = "")
        {
            return new StreamableVideoModel
            {
                IsErrorModel = true,
                Title = "Unable to Load Video",
                DefaultThumbnailUrl = $"http://i.imgur.com/{CantLoadThumbnailBaseUrl}.png",
                VideoFullUrl = $"http://www.streamable.com/videos/{videoId}"
            };
        }
    }
}