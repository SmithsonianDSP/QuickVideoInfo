using System;
using System.Collections.Generic;

namespace YTII.APIs.Models
{
    public class YouTubeVideoModel
    {
        public string VideoId { get; internal set; }
        public string Title { get; internal set; }
        public string Description { get; internal set; }
        public string ChannelTitle { get; internal set; }

        public string VideoDurationISO8601
        {
            set
            {
                try { VideoDuration = System.Xml.XmlConvert.ToTimeSpan(value); }
                catch { }
            }
        }

        public TimeSpan? VideoDuration { get; internal set; }

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

        public string DefaultThumbnailUrl { get; internal set; }
        public string MediumThumbnailUrl { get; internal set; }
        public string HighThumbnailUrl { get; internal set; }
        public string StandardThumbnailUrl { get; internal set; }
        public string MaxResThumbnailUrl { get; internal set; }

        public DateTime PublishedAt { get; internal set; }

        public int? ViewCount { get; internal set; }
        public string ViewCountString { get => ViewCount.PrettyNumberString(); }

        public int? LikeCount { get; internal set; }
        public string LikeCountString { get => LikeCount.PrettyNumberString(); }

        public int? DislikeCount { get; internal set; }
        public string DislikeCountString { get => DislikeCount.PrettyNumberString(); }

        public int? FavoriteCount { get; internal set; }

        public int? CommentCount { get; internal set; }

        public IEnumerable<string> Tags { get; internal set; } = new List<string>();

    }

    static class YouTubeVideoModelExtensions
    {
        internal static string PrettyNumberString(this int? num)
        {
            var number = num ?? 0;

            if (number > 1000000)
                return Math.Floor(number / 1000000D).ToString() + "M";
            else if (number > 1000)
                return Math.Floor(number / 1000D).ToString() + "K";
            else
                return number.ToString();
        }
    }
}
