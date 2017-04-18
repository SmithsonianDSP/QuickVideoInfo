using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using YTII.Droid.App;
using YTII.ModelFactory.Models;
using System.Linq;
using Android.Net;
using System.Collections.Generic;
using Android.Content;
using Android.Util;
using Java.Lang;
using Android.Views;
using System.Threading.Tasks;
using AndroidAnimations = Android.Animation;
using AndroidPM = Android.Content.PM;

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
    public class MainActivity : Activity
    {
        protected string videoId;
        protected LruCache mMemoryCache { get => retainedFragment.MRetainedCache; }
        private YouTubeModelCache ModelCache { get => retainedFragment.YouTubeVideoModelCache; }

        protected RetainFragment retainedFragment;



        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            GridLayout main;

            if (GetScreenAspectRatio() > 1)
            {
                SetContentView(Resource.Layout.MainLandscape);
                main = FindViewById<GridLayout>(Resource.Id.mainLayout2);
            }
            else
            {
                SetContentView(Resource.Layout.Main);
                main = FindViewById<GridLayout>(Resource.Id.mainLayout1);
            }

            main.LayoutTransition?.DisableTransitionType(AndroidAnimations.LayoutTransitionType.Appearing);
            main.LayoutTransition?.SetDuration(AndroidAnimations.LayoutTransitionType.Disappearing, 2500);

            SetScreenSizeValues();
            SetEventHandlers();
            InitializeCache();

            await LoadVideo();
        }

        protected virtual async Task LoadVideo()
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
                    LoadVideoDetails(ref vid);
                    LoadVideoThumbnail(ref vid);
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Square.Picasso.Picasso.With(this).CancelTag(this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            Square.Picasso.Picasso.With(this).PauseTag(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Square.Picasso.Picasso.With(this).ResumeTag(this);
        }

        protected virtual void InitializeCache()
        {
            retainedFragment = RetainFragment.FindOrCreateRetainFragment(FragmentManager);
            if (retainedFragment.HavePreferencesBeenChecked)
            {
                VerifyLauncherEnabledSettings();
                retainedFragment.HavePreferencesBeenChecked = true;
            }

        }

        protected virtual void SetEventHandlers()
        {
            var closeButton = FindViewById<Button>(Resource.Id.closeButton);
            closeButton.Click += CloseButton_Click;

            var openButton = FindViewById<Button>(Resource.Id.button1);
            openButton.Click += OpenButton_Click;

            var aboutButton = FindViewById<ImageButton>(Resource.Id.imageButton);
            aboutButton.Click += AboutButton_Click;
        }

        protected virtual string GetVideoIdFromIntentDataString(string intentDataString)
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


        protected virtual void VerifyLauncherEnabledSettings()
        {
            var componentToEnable = new ComponentName(Constants.PackageName, YTII.Droid.App.LauncherActivity.FullActivityName);
            var componentStatus = PackageManager.GetComponentEnabledSetting(componentToEnable);

            if (componentStatus != AndroidPM.ComponentEnabledState.Enabled || componentStatus != AndroidPM.ComponentEnabledState.Default)
            {
                // If the Launcher activity is disabled, verify that user settings supposed to be disabled 
                if (UserSettings.IsLauncherIconShown)
                    PackageManager.SetComponentEnabledSetting(componentToEnable, AndroidPM.ComponentEnabledState.Enabled, AndroidPM.ComponentEnableOption.DontKillApp);
            }
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
        protected virtual void LoadVideoDetails(ref YouTubeVideoModel video)
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

                LoadVideoThumbnail(ref video);
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(LoadVideoDetails)}", ex.Message);
                UnableToLoadVideoInfo(ex);
            }
        }

        protected virtual void LoadVideoThumbnail(ref YouTubeVideoModel video)
        {
            var imgFrame = FindViewById<RelativeLayout>(Resource.Id.mediaFrame1);
            var imgHost = FindViewById<ImageView>(Resource.Id.imageView);
            var cantLoadThumbnail = GetDrawable(Resource.Drawable.CantLoadVideo);

            try
            {
                Bitmap thumb = TryGetCache(video.VideoId) as Bitmap;

                if (thumb != null)
                {
                    SetScreenSizeValues();

                    var prog = FindViewById<ProgressBar>(Resource.Id.progressSpinner);

                    prog.Visibility = ViewStates.Invisible;
                    imgHost.Visibility = ViewStates.Visible;

                    imgHost.SetImageBitmap(thumb);

                    Square.Picasso.Picasso.With(BaseContext)
                                          .Load(GetThumbnailUrl(ref video))
                                          .Tag(nameof(MainActivity))
                                          .NoFade()
                                          .Placeholder(imgHost.Drawable)
                                          .Error(cantLoadThumbnail)
                                          .Fit()
                                          .CenterCrop()
                                          .Into(imgHost, PicassoOnSuccess, PicassoOnError);


                    Log.Info($"YTII.{nameof(LoadVideoThumbnail)}", "Thumbail Loaded From Cache");
                }
                else
                {
                    try
                    {
                        var thumbnailUrl = GetThumbnailUrl(ref video);

                        Log.Debug("YTII.ThumbnailURL", thumbnailUrl);

                        imgHost.SetWillNotCacheDrawing(true);

                        Square.Picasso.Picasso.With(BaseContext)
                                              .Load(thumbnailUrl)
                                              .Tag(nameof(MainActivity))
                                              .Error(cantLoadThumbnail)
                                              .NoFade()
                                              .Transform(new TrimBitmapHeightTransform())
                                              .Fit()
                                              .CenterCrop()
                                              .Into(imgHost, PicassoOnSuccess, PicassoOnError);

                        Log.Info($"YTII.{nameof(LoadVideoThumbnail)}", "Loaded Thumbnail into ImageView");

                        imgHost.SetWillNotCacheDrawing(false);
                        imgHost.BuildDrawingCache(false);
                        var bmc = imgHost.GetDrawingCache(false);
                        retainedFragment.MRetainedCache.Put(video.VideoId, Bitmap.CreateBitmap(bmc));
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"YTII.{nameof(LoadVideoThumbnail)}.Download", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(LoadVideoThumbnail)}.Main", ex.Message);
            }


        }

        protected virtual object TryGetCache(string videoId)
        {

            try
            {
                return mMemoryCache.Get(videoId);
            }
            catch
            {
                return null;
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
        protected virtual string GetThumbnailUrl(ref YouTubeVideoModel vid)
        {
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




        protected virtual void UnableToLoadVideoInfo(Exception ex = null)
        {
            var textBlock = FindViewById<TextView>(Resource.Id.textView1);
            textBlock.Text = "Unable to Load Video Information";
            var spinner = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
            spinner.Visibility = ViewStates.Invisible;
#if DEBUG
            if (ex != null)
            {
                var toast = Toast.MakeText(this.ApplicationContext, ex.Message, ToastLength.Long);
                toast.Show();
            }
#endif
        }

        protected void PicassoOnSuccess()
        {
            try
            {
                var imgHost = FindViewById<ImageView>(Resource.Id.imageView);
                imgHost.SetMaxHeight(ImgHeight);
                imgHost.Visibility = ViewStates.Visible;
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(PicassoOnSuccess)}.imgHost", ex.Message);
            }
            try
            {
                var spinner = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
                spinner.Visibility = ViewStates.Invisible;
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(PicassoOnSuccess)}.progressSpinner", ex.Message);
            }
        }

        protected void PicassoOnError()
        {
            var spinner = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
            spinner.Visibility = ViewStates.Gone;
        }



        protected virtual void AboutButton_Click(object sender, System.EventArgs e)
        {
            StartActivity(typeof(AboutActivity));
        }

        protected virtual void CloseButton_Click(object sender, System.EventArgs e)
        {
            retainedFragment.retainedVideo = null;
            FinishAndRemoveTask();
        }

        protected virtual void OpenButton_Click(object sender, System.EventArgs e)
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




        /// <summary>
        /// Provides compatibility for pre-Android 5.0 devices that do not have Activity.FinishAndRemoveTask() 
        /// </summary>
        public override void FinishAndRemoveTask()
        {
            try
            {
                base.FinishAndRemoveTask();
            }
            catch (NoSuchMethodError ex)
            {
                base.Finish();
            }
        }



        #region ScreenSizeBehavior

        /// <summary>
        /// Sets dimensions for the ImageHost frame based on the current screen dimensions
        /// </summary>
        protected void SetScreenSizeValues()
        {
            var size = GetScreenDimensions();
            ScreenWidth = size.Width;
            ScreenHeight = size.Height;
        }

        private Size GetScreenDimensions()
        {
            var rect = new Rect();
            WindowManager.DefaultDisplay.GetRectSize(rect);
            return new Size(rect.Width(), rect.Height());
        }

        private double GetScreenAspectRatio()
        {
            Size dims = GetScreenDimensions();
            return (double)dims.Width / (double)dims.Height;
        }

        int ScreenWidth { get; set; } = 480;
        int ScreenHeight { get; set; } = 850;

        int ImgWidth { get => (int)Math.Max(Math.Rint(ScreenWidth * 0.965), 480); }
        int ImgHeight { get => (int)Math.Rint(Math.Min(ImgWidth * ScreenAspect, ScreenHeight * 0.75)); }
        double ScreenAspect { get => GetScreenAspectRatio(); }
        #endregion
    }




}

