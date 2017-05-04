#region file_header

// QuickVideoInfo - YTII.Android.App - VimeoVideoInfoActivity.cs
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
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using YTII.Droid.App.Caches;
using YTII.ModelFactory.Models;
using Exception = Java.Lang.Exception;

namespace YTII.Droid.App.Activities
{
    [Activity(Label = "Quick Video Info", Theme = "@style/TranslucentActivity", MainLauncher = false, Icon = "@drawable/icon")]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "http", DataHost = "*.vimeo.com", DataPathPrefix = "", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "http", DataHost = "vimeo.com", DataPathPrefix = "", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "https", DataHost = "vimeo.com", DataPathPrefix = "", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "https", DataHost = "*.vimeo.com", DataPathPrefix = "", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    public class VimeoVideoInfoActivity : BaseVideoInfoActivity<VimeoVideoModel>
    {
        VimeoVideoModel vid;

        /// <summary>
        ///     The name of the activity. Used for identifying it in log messages
        /// </summary>
        protected override string ActivityName => nameof(VimeoVideoInfoActivity);

        /// <summary>
        ///     This is a prefix used to distinguish the origin source of thumbnails (e.g., YT[videoID] for YouTube, ST[videoID]
        ///     for Streamable.com)
        /// </summary>
        protected override string TypePrefix => "V";

        /// <summary>
        ///     The <see cref="Caches.VideoModelCache{T}" /> where the results of recently previewed video models are stored
        /// </summary>
        protected override VideoModelCache<VimeoVideoModel> ModelCache => retainedFragment.VimeoVideoCache;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Log.Debug("YTII.StreamableIntentUrl", Intent.DataString);
            Log.Debug("YTII.StreamableIntentUrl", GetVideoIdFromIntentDataString(Intent.DataString));

            int videoId;
            // FIRST NEED TO CONFIRM THIS IS A VIMEO.COM VIDEO URL; ALL OTHERS REDIRECT TO BROWSER
            if (!int.TryParse(GetVideoIdFromIntentDataString(Intent.DataString), out videoId))
            {
                SendUrlToBrowser(Intent.DataString);
                FinishAfterTransition();
                FinishAndRemoveTask();
                return;
            }


            var extraDetailsRow = FindViewById<GridLayout>(Resource.Id.gridDetails);
            extraDetailsRow.Visibility = ViewStates.Gone;
        }

        /// <summary>
        ///     This method is where the actual work of requesting/loading the video info from the API
        /// </summary>
        /// <returns>N/A</returns>
        protected override async Task LoadVideo()
        {
            var sourceText = FindViewById<TextView>(Resource.Id.sourceText);
            sourceText.Text = @"Vimeo.com";

            SetYouTubeAuthItems();

            try
            {
                videoId = GetVideoIdFromIntentDataString(Intent.DataString);

                if (ModelCache.IsCached(videoId))
                    vid = ModelCache.GetItem(videoId);
                else
                    vid = await VideoInfoRequestor.GetVimeoVideoModel(videoId).ConfigureAwait(true);

                if (vid != null)
                {
                    vid.VideoFullUrl = Intent.DataString;
                    LoadVideoDetails(vid);

                    if (!vid.IsErrorModel)
                        ModelCache.Add(vid);
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
        ///     Populates the activity controls with the details from the supplied <see cref="IVideoModel" />
        /// </summary>
        /// <param name="video">The <see cref="IVideoModel" /> to load the details into the layout for</param>
        protected override void LoadVideoDetails(VimeoVideoModel video)
        {
            try
            {
                var videoTitle = FindViewById<TextView>(Resource.Id.textView1);

                if (video.Title != string.Empty)
                    videoTitle.Text = video.Title;
                else
                    videoTitle.Text = "[ No Title ]";

                var videoDuration = FindViewById<TextView>(Resource.Id.videoDuration);
                videoDuration.Text = video.VideoDurationString;

                LoadVideoThumbnail(video);
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(LoadVideoDetails)}", ex.Message);
                UnableToLoadVideoInfo(ex);
            }
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
        ///     Processes the intent data string (URL) and returns the video ID
        /// </summary>
        /// <param name="intentDataString">The <see cref="P:Android.Content.Intent.DataString" /> passed to the activity.</param>
        /// <returns>The Video ID used to identify the item to request information from the API for</returns>
        protected override string GetVideoIdFromIntentDataString(string intentDataString)
        {
            return intentDataString.Substring(intentDataString.LastIndexOf(".com/", StringComparison.InvariantCulture) + 5).TrimEnd('/');
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
        protected override string GetThumbnailUrl(ref VimeoVideoModel vid)
        {
            var matchLink = string.Empty;

            if (UserSettings.ThumbnailQuality == 0) // Max Thumbnail Quality
                matchLink = vid.Thumbnails.Sizes.OrderByDescending(p => p.Width).FirstOrDefault()?.Link;

            else if (UserSettings.ThumbnailQuality == 4) // Lowest Thumbnail Quality
                matchLink = vid.Thumbnails.Sizes.OrderBy(p => p.Width).FirstOrDefault()?.Link;

            return string.IsNullOrEmpty(matchLink) ? vid.DefaultThumbnailUrl : matchLink;
        }

        /// <summary>
        ///     The method to execute when the user wants to open the currently previewed video. Implementation will vary depending
        ///     on the explicit <see cref="IVideoModel" /> type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OpenButton_Click(object sender, EventArgs e)
        {
            SendUrlToBrowser(vid.VideoFullUrl);
            FinishAfterTransition();
            FinishAndRemoveTask();
        }
    }
}