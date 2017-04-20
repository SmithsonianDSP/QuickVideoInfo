using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YTII.ModelFactory.Models;

namespace YTII.ModelFactory.Factories
{
    public static class StreamableVideoFactory
    {
        public static StreamableVideoModel GetModelFromJson(string payload)
        {
            try
            {
                JToken result = JsonConvert.DeserializeObject<dynamic>(payload);
                JToken fileInfo = result.Value<JObject>("files")?.First?.First;

                string videoFullUrl = "https://" + result.Value<string>("url");

                return new StreamableVideoModel()
                {
                    VideoId = videoFullUrl.Substring(videoFullUrl.LastIndexOf(".com") + 5),
                    DefaultThumbnailUrl = "http:" + result.Value<string>("thumbnail_url"),
                    Title = result.Value<string>("title"),
                    VideoFullUrl = videoFullUrl,
                    VideoDurationSeconds = fileInfo?.Value<float?>("duration")
                };
            }
            catch
            {
                return GetCannotLoadVideoModel();
            }

        }


        private static readonly string CantLoadThumbnailBaseUrl = "Jn2grYW";

        public static StreamableVideoModel GetCannotLoadVideoModel(string videoId = "")
        {
            return new StreamableVideoModel()
            {
                IsErrorModel = true,
                Title = "Unable to Load Video",
                DefaultThumbnailUrl = $"http://i.imgur.com/{CantLoadThumbnailBaseUrl}.png",
                VideoFullUrl = $"http://www.streamable.com/videos/{videoId}"
            };
        }



    }
}
