using System;
using System.Collections.Generic;

namespace YTII.ModelFactory.Models
{
    public class YouTubeVideoModel : IVideoModel
    {

        #region Interface Implementations

        public string VideoId { get; set; }

        public string VideoFullUrl { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public TimeSpan? VideoDuration { get; set; }
        public string VideoDurationString
        {
            get
            {
                var videoDurationString = VideoDuration.ToString().TrimStart(new char[] { '0', ':' });
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
                return videoDurationString;
            }
        }
        public string DefaultThumbnailUrl { get; set; }

        #endregion

        public string ChannelTitle { get; set; }

        public string VideoDurationISO8601
        {
            set
            {
                try { VideoDuration = System.Xml.XmlConvert.ToTimeSpan(value); }
                catch { }
            }
        }


        public string MediumThumbnailUrl { get; set; }
        public string HighThumbnailUrl { get; set; }
        public string StandardThumbnailUrl { get; set; }
        public string MaxResThumbnailUrl { get; set; }

        public DateTime PublishedAt { get; set; }

        public int? ViewCount { get; set; }
        public string ViewCountString { get => ViewCount.PrettyNumberString(); }

        public int? LikeCount { get; set; }
        public string LikeCountString { get => LikeCount.PrettyNumberString(); }

        public int? DislikeCount { get; set; }
        public string DislikeCountString { get => DislikeCount.PrettyNumberString(); }

        public int? FavoriteCount { get; set; }

        public int? CommentCount { get; set; }

        public IEnumerable<string> Tags { get; internal set; } = new List<string>();


    }

    static class YouTubeVideoModelExtensions
    {
        internal static string PrettyNumberString(this int? num)
        {
            var number = num ?? 0;

            if (number > 1000000)
                return Math.Floor(number / 1000000D).ToString() + "." + Math.Floor((number % 1000000D) / 100000D) + "M";

            else if (number > 1000)
                return Math.Floor(number / 1000D).ToString() + "." + Math.Floor((number % 1000D) / 100D) + "K";

            else
                return number.ToString();
        }
    }
}
