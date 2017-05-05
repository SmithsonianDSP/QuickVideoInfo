#region file_header

// QuickVideoInfo - YTII.ModelFactory - VimeoVideoFactory.cs
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
using System.Diagnostics;
using Newtonsoft.Json;
using YTII.ModelFactory.Models;

namespace YTII.ModelFactory.Factories
{
    public static class VimeoVideoFactory
    {
        public static VimeoVideoModel GetVimeoVideoModelFromJson(string json)
        {
            try
            {
                if (json == "\"Not Found\"")
                    return GetErrorLoadingVideoModel("Video Not Found");

                return JsonConvert.DeserializeObject<VimeoVideoModel>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return GetErrorLoadingVideoModel(ex.Message);
            }
        }


        public static VimeoVideoModel GetErrorLoadingVideoModel(string errorMessage = "There was a problem loading video info")
        {
            return new VimeoVideoModel
            {
                Title = errorMessage,
                Description = errorMessage,
                Thumbnails = new VimeoVideoModel.Pictures
                {
                    Active = true,
                    Sizes = new List<VimeoVideoModel.Pictures.Size>
                                        {
                                            new VimeoVideoModel.Pictures.Size
                                            {
                                                Link = VideoModelExtensions.CantLoadThumbnailImageUrl
                                            }
                                        }
                },
                VideoStats = new VimeoVideoModel.Stats { Plays = 0 },
                CreatedTime = DateTime.Today,
                IsErrorModel = true
            };
        }
    }
}