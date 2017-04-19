using Android.App;
using Android.Widget;
using YTII.ModelFactory.Models;
using Android.Net;
using Android.Content;
using Android.Util;
using Java.Lang;
using System.Threading.Tasks;

namespace YTII.Droid.App
{
    [Activity(Label = "Quick Video Info", Theme = "@style/CustomTheme", MainLauncher = false, Icon = "@drawable/icon")]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "http", DataHost = "*.youtube.com", DataPathPrefix = "/watch",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "https", DataHost = "*.youtube.com", DataPathPrefix = "/watch",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "http", DataHost = "youtube.com", DataPathPrefix = "/watch",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "https", DataHost = "youtube.com", DataPathPrefix = "/watch",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "http", DataHost = "youtu.be",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "https", DataHost = "youtu.be",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    public class YouTubeVideoInfoActivity : BaseVideoInfoActivity<YouTubeVideoModel>
    {
        protected override string ActivityName { get => nameof(YouTubeVideoInfoActivity); }

        protected override Caches.VideoModelCache<YouTubeVideoModel> ModelCache { get => retainedFragment.YouTubeVideoModelCache; }

        protected override async Task LoadVideo()
        {
            try
            {
                videoId = GetVideoIdFromIntentDataString(Intent.DataString);

                YouTubeVideoModel vid;

                if (ModelCache.IsCached(videoId))
                    vid = ModelCache.GetItem(videoId) as YouTubeVideoModel;

                else
                {
                    SetYouTubeAuthItems();
                    vid = await VideoInfoRequestor.GetYouTubeVideoModel(videoId).ConfigureAwait(true);
                }

                if (vid != null)
                {
                    ModelCache.Add(vid);
                    LoadVideoDetails(vid);
                    LoadVideoThumbnail(vid);
                }
                else
                    UnableToLoadVideoInfo();
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(OnCreate)}", ex.Message);
                UnableToLoadVideoInfo(ex);
            }
        }

        protected override string GetVideoIdFromIntentDataString(string intentDataString)
        {
            int idIndex;
            string vidId = string.Empty;
            var da = intentDataString;

            if (da != null)
            {
                if (da.Contains(@"watch"))
                {
                    idIndex = da.LastIndexOf("v=") + 2;
                    vidId = da.Substring(idIndex, 11);
                }
                else
                {
                    idIndex = da.LastIndexOf(@"/") + 1;
                    vidId = da.Substring(idIndex, 11);
                }
            }
            return vidId;
        }

        protected void SetYouTubeAuthItems()
        {
            if (VideoInfoRequestor.Thumbprint == null || VideoInfoRequestor.Thumbprint == string.Empty)
                VideoInfoRequestor.Thumbprint = SignatureVerification.GetSignature(PackageManager, PackageName);

            VideoInfoRequestor.PackageName = PackageName;
        }

        /// <summary>
        /// Populates the activity controls with the details from the supplied YouTubeVideo model
        /// </summary>
        /// <param name="video"></param>
        protected override void LoadVideoDetails(YouTubeVideoModel video)
        {
            try
            {
                var videoTitle = FindViewById<TextView>(Resource.Id.textView1);
                videoTitle.Text = video.Title;

                var videoDuration = FindViewById<TextView>(Resource.Id.videoDuration);
                videoDuration.Text = video.VideoDurationString;
                videoDuration.BringToFront();

                var viewCount = FindViewById<TextView>(Resource.Id.viewCount);
                viewCount.Text = video.ViewCountString;

                var likeCount = FindViewById<TextView>(Resource.Id.likeCount);
                likeCount.Text = video.LikeCountString;

                var dislikeCount = FindViewById<TextView>(Resource.Id.dislikeCount);
                dislikeCount.Text = video.DislikeCountString;

                LoadVideoThumbnail(video);
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(LoadVideoDetails)}", ex.Message);
                UnableToLoadVideoInfo(ex);
            }
        }

        /// <summary>
        /// Returns the most appropriate video Thumbnail URL
        /// </summary>
        /// <remarks>
        /// Refactored this out into its own function, for now, in anticipation of eventually allowing users to set a preferred thumbnail size
        /// </remarks>
        /// <param name="vid">The YouTubeVideoModel whose thumbnail URL is desired</param>
        /// <returns>a URL of the thumbnail to load</returns>
        protected override string GetThumbnailUrl(ref YouTubeVideoModel vid)
        {
            var vid = video as YouTubeVideoModel;

            string thumbnailUrl = null;

            if (UserSettings.ThumbnailQuality == 0)
                thumbnailUrl = vid.MaxResThumbnailUrl;

            if (UserSettings.ThumbnailQuality <= 1)
                thumbnailUrl = thumbnailUrl ?? vid.StandardThumbnailUrl;

            if (UserSettings.ThumbnailQuality <= 2)
                thumbnailUrl = thumbnailUrl ?? vid.HighThumbnailUrl;

            if (UserSettings.ThumbnailQuality <= 3)
                thumbnailUrl = thumbnailUrl ?? vid.MediumThumbnailUrl;

            if (UserSettings.ThumbnailQuality <= 4)
                thumbnailUrl = thumbnailUrl ?? vid.DefaultThumbnailUrl;

            return thumbnailUrl;
        }

        protected override void OpenButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                var i = new Intent(Intent.ActionView, Uri.Parse("vnd.youtube:" + videoId));
                i.AddFlags(ActivityFlags.NewTask);
                ApplicationContext.StartActivity(i);
                FinishAfterTransition();
                FinishAndRemoveTask();
                retainedFragment.retainedVideo = null;
            }
            catch (Exception ex)
            {
                Log.Error("YTII", ex.Message);
                var toast = Toast.MakeText(this.ApplicationContext, "Failed to intent to YouTube App!", ToastLength.Long);
                toast.Show();
            }
        }
    }

}

