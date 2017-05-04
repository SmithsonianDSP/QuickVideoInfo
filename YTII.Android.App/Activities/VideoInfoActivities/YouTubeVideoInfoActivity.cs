#region file_header

// QuickVideoInfo - YTII.Android.App - YouTubeVideoInfoActivity.cs
// 
// This file is licensed to you under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express 
// or implied.  See the License for the specific language governing permissions and limitations under the License.
//  

#endregion

using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using YTII.Droid.App.Caches;
using YTII.ModelFactory.Models;
using Exception = Java.Lang.Exception;
using Uri = Android.Net.Uri;

namespace YTII.Droid.App
{
    [Activity(Label = "Quick Video Info", Theme = "@style/TranslucentActivity", MainLauncher = false, Icon = "@drawable/icon")]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "http", DataHost = "youtu.be", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "https", DataHost = "youtu.be", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "http", DataHost = "*.youtube.com", DataPathPrefix = "/watch", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "https", DataHost = "*.youtube.com", DataPathPrefix = "/watch", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "http", DataHost = "youtube.com", DataPathPrefix = "/watch", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "https", DataHost = "youtube.com", DataPathPrefix = "/watch", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    public class YouTubeVideoInfoActivity : BaseVideoInfoActivity<YouTubeVideoModel>
    {
        /// <summary>
        ///     The name of the activity. Used for identifying it in log messages
        /// </summary>
        protected override string ActivityName => nameof(YouTubeVideoInfoActivity);

        /// <summary>
        ///     This is a prefix used to distinguish the origin source of thumbnails (e.g., YT[videoID] for YouTube, ST[videoID]
        ///     for Streamable.com)
        /// </summary>
        protected override string TypePrefix => @"YT";

        /// <summary>
        ///     The <see cref="Caches.VideoModelCache{T}" /> where the results of recently previewed video models are stored
        /// </summary>
        protected override VideoModelCache<YouTubeVideoModel> ModelCache => retainedFragment.YouTubeVideoModelCache;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        /// <summary>
        ///     This method is where the actual work of requesting/loading the video info from the API
        /// </summary>
        /// <returns>N/A</returns>
        protected override async Task LoadVideo()
        {
            var sourceText = FindViewById<TextView>(Resource.Id.sourceText);
            sourceText.Text = @"youtube.com";

            try
            {
                videoId = GetVideoIdFromIntentDataString(Intent.DataString);

                YouTubeVideoModel vid;

                if (ModelCache.IsCached(videoId))
                {
                    vid = ModelCache.GetItem(videoId);
                }

                else
                {
                    SetYouTubeAuthItems();
                    vid = await VideoInfoRequestor.GetYouTubeVideoModel(videoId).ConfigureAwait(true);
                }

                if (vid != null)
                {
                    ModelCache.Add(vid);
                    LoadVideoDetails(vid);
                }
                else
                {
                    UnableToLoadVideoInfo();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(OnCreate)}", ex.Message);
                UnableToLoadVideoInfo(ex);
            }
        }

        /// <summary>
        ///     Processes the intent data string (URL) and returns the video ID
        /// </summary>
        /// <param name="intentDataString">The <see cref="P:Android.Content.Intent.DataString" /> passed to the activity.</param>
        /// <returns>The Video ID used to identify the item to request information from the API for</returns>
        protected override string GetVideoIdFromIntentDataString(string intentDataString)
        {
            int idIndex;
            var vidId = string.Empty;
            var da = intentDataString;

            if (da != null)
                if (da.Contains(@"watch"))
                {
                    idIndex = da.LastIndexOf(@"v=", StringComparison.InvariantCulture) + 2;
                    vidId = da.Substring(idIndex, 11);
                }
                else
                {
                    idIndex = da.LastIndexOf(@"/", StringComparison.InvariantCulture) + 1;
                    vidId = da.Substring(idIndex, 11);
                }
            return vidId;
        }

        /// <summary>
        ///     Attaches the App's thumbprint/signature for API authorization purposes
        /// </summary>
        protected void SetYouTubeAuthItems()
        {
            if (string.IsNullOrEmpty(VideoInfoRequestor.Thumbprint))
                VideoInfoRequestor.Thumbprint = SignatureVerification.GetSignature(PackageManager, PackageName);

            VideoInfoRequestor.PackageName = PackageName;
        }

        /// <summary>
        ///     Populates the activity controls with the details from the supplied <see cref="IVideoModel" />
        /// </summary>
        /// <param name="video">The <see cref="IVideoModel" /> to load the details into the layout for</param>
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

                RunOnUiThread(() => LoadVideoThumbnail(video));
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(LoadVideoDetails)}", ex.Message);
                UnableToLoadVideoInfo(ex);
            }
        }

        /// <summary>
        ///     Returns the most appropriate video Thumbnail URL
        /// </summary>
        /// <remarks>
        ///     This was refactored out into its own function to accomodate allowing the user to define a preferred thumbnail
        ///     quality
        /// </remarks>
        /// <param name="vid">The <see cref="IVideoModel" /> whose thumbnail URL is desired</param>
        /// <returns>A URL of the thumbnail to load</returns>
        protected override string GetThumbnailUrl(ref YouTubeVideoModel vid)
        {
            switch (UserSettings.ThumbnailQuality)
            {
                case 0:
                    return vid.MaxResThumbnailUrl
                           ?? vid.StandardThumbnailUrl
                           ?? vid.HighThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl;
                case 1:
                    return vid.StandardThumbnailUrl
                           ?? vid.HighThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl;
                case 2:
                    return vid.HighThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl;
                case 3:
                    return vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl
                           ?? vid.HighThumbnailUrl;
                case 4:
                    return vid.DefaultThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.HighThumbnailUrl;
                default:
                    return vid.StandardThumbnailUrl
                           ?? vid.HighThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl;
            }
        }

        /// <summary>
        ///     The method to execute when the user wants to open the currently previewed video. Implementation will vary depending
        ///     on the explicit <see cref="IVideoModel" /> type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OpenButton_Click(object sender, EventArgs e)
        {
            try
            {
                var i = new Intent(Intent.ActionView, Uri.Parse(@"vnd.youtube:" + videoId));
                i.AddFlags(ActivityFlags.NewTask);
                ApplicationContext.StartActivity(i);
                FinishAfterTransition();
                FinishAndRemoveTask();
                retainedFragment.retainedVideo = null;
            }
            catch (Exception ex)
            {
                Log.Error("YTII", ex.Message);
                var toast = Toast.MakeText(ApplicationContext, "Failed to intent to YouTube App!", ToastLength.Long);
                toast.Show();
            }
        }
    }
}