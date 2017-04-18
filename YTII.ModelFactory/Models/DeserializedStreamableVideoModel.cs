using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YTII.ModelFactory.Models
{

    public class DeserializedStreamableVideoModel
    {
        public int? status { get; set; }
        public Files files { get; set; }
        public string thumbnail_url { get; set; }
        public object source { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public object message { get; set; }
        public int percent { get; set; }
    }

    public class Files
    {
        public Mp4 mp4 { get; set; }
        public Mp4Mobile mp4mobile { get; set; }
    }

    public class Mp4
    {
        public int? status { get; set; }
        public int? width { get; set; }
        public string url { get; set; }
        public int? bitrate { get; set; }
        public float? duration { get; set; }
        public int? size { get; set; }
        public int? framerate { get; set; }
        public int? height { get; set; }
    }

    public class Mp4Mobile
    {
        public int? status { get; set; }
        public int? width { get; set; }
        public string url { get; set; }
        public int? bitrate { get; set; }
        public float? duration { get; set; }
        public int? size { get; set; }
        public int? framerate { get; set; }
        public int? height { get; set; }
    }

}
