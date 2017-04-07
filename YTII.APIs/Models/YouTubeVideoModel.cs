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
            get => VideoDuration?.ToString().TrimStart(new char[] { '0', ':' }) ?? string.Empty;
        }

        public string DefaultThumbnailUrl { get; internal set; }
        public string MediumThumbnailUrl { get; internal set; }
        public string HighThumbnailUrl { get; internal set; }
        public string StandardThumbnailUrl { get; internal set; }
        public string MaxResThumbnailUrl { get; internal set; }

        public DateTime PublishedAt { get; internal set; }

        public int? ViewCount { get; internal set; }
        public int? LikeCount { get; internal set; }
        public int? DislikeCount { get; internal set; }
        public int? FavoriteCount { get; internal set; }
        public int? CommentCount { get; internal set; }

        public IEnumerable<string> Tags { get; internal set; } = new List<string>();

    }
}
