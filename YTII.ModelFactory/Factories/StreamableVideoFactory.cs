using Newtonsoft.Json;
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
                var result = JsonConvert.DeserializeObject<DeserializedStreamableVideoModel>(payload);
                return new StreamableVideoModel(result);
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
                Title = "Unable to Load Video",
                DefaultThumbnailUrl = $"http://i.imgur.com/{CantLoadThumbnailBaseUrl}.png",
                VideoFullUrl = $"http://www.streamable.com/videos/{videoId}"
            };
        }



    }
}
