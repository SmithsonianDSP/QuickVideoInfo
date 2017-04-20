using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YTII.ModelFactory.Models
{
    public class StreamableVideoModel : IVideoModel
    {
        public bool IsErrorModel { get; internal set; } = false;

        public string VideoId { get; set; }
        public string VideoFullUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DefaultThumbnailUrl { get; set; }

        public float? VideoDurationSeconds { get; set; }
        public TimeSpan? VideoDuration { get => TimeSpan.FromSeconds((int)(VideoDurationSeconds ?? 0)); set => VideoDurationSeconds = (float?)(value ?? TimeSpan.FromSeconds(0)).TotalSeconds; }

        public string VideoDurationString
        {
            get
            {
                var videoDurationString = (VideoDuration ?? TimeSpan.FromMinutes(0)).ToString().TrimStart(new char[] { '0', ':' });
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
                return videoDurationString ?? string.Empty;
            }
        }




        public StreamableVideoModel() { }


        public StreamableVideoModel(DeserializedStreamableVideoModel baseModel)
        {
            VideoId = baseModel.url.Substring(baseModel.url.LastIndexOf(".com/") + 5);
            VideoFullUrl = "https://" + baseModel.url;
            DefaultThumbnailUrl = "https:" + baseModel.thumbnail_url;
            Title = baseModel.title ?? "[No Title]";
            VideoDurationSeconds = baseModel.files.mp4.duration;
            //VideoDuration = TimeSpan.FromSeconds((int)(baseModel.files.mp4?.duration ?? 0));
        }



    }
}
