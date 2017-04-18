using System;

namespace YTII.ModelFactory.Models
{
    public interface IVideoModel
    {
        string VideoId { get; set; }

        string VideoFullUrl { get; set; }

        string Title { get; set; }

        string Description { get; set; }

        string DefaultThumbnailUrl { get; set; }


        TimeSpan? VideoDuration { get; set; }
        string VideoDurationString { get; }

    }
}