using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YTII.ModelFactory.Models
{
    public class StreamableVideoModel : IVideoModel
    {
        public string VideoId { get; set; }
        public string VideoFullUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DefaultThumbnailUrl { get; set; }

        public float? VideoDurationSeconds { get; set; }
        public TimeSpan? VideoDuration { get; set; }

        public string VideoDurationString { get; }




        public StreamableVideoModel() { }


        public StreamableVideoModel(DeserializedStreamableVideoModel baseModel)
        {
            VideoId = baseModel.url.Substring(baseModel.url.LastIndexOf('/'));
            VideoFullUrl = "https://" + baseModel.url;
            DefaultThumbnailUrl = "https:" + baseModel.thumbnail_url;
            Title = baseModel.title ?? "[No Title]";
            VideoDurationSeconds = baseModel.files.mp4.duration;
            VideoDuration = TimeSpan.FromSeconds(VideoDurationSeconds ?? 0);
        }



    }
}
