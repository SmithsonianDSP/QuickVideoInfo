using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using YTII.ModelFactory.Models;
using Android.Content;
using Android.Util;
using Java.Lang;
using Android.Views;
using System.Threading.Tasks;
using AndroidAnimations = Android.Animation;
using AndroidPM = Android.Content.PM;
using Android.Net;

namespace YTII.Droid.App
{
    public abstract class BaseVideoInfoActivity<T> : Activity
        where T : IVideoModel
    {

        #region Abstract Properties, Fields, and Methods that must be overridden by classes that inheirit from this

        /// <summary>
        /// The name of the activity. Used for identifying it in log messages
        /// </summary>
        protected abstract string ActivityName { get; }

        /// <summary>
        /// This is a prefix used to distinguish the origin source of thumbnails (e.g., YT[videoID] for YouTube, ST[videoID] for Streamable.com)
        /// </summary>
        protected abstract string TypePrefix { get; }

        /// <summary>
        /// The <see cref="Caches.VideoModelCache{T}"/> where the results of recently previewed video models are stored
        /// </summary>
        protected abstract Caches.VideoModelCache<T> ModelCache { get; }

        /// <summary>
        /// Processes the intent data string (URL) and returns the video ID
        /// </summary>
        /// <param name="intentDataString">The <see cref="Intent.DataString"/> passed to the activity.</param>
        /// <returns>The Video ID used to identify the item to request information from the API for</returns>
        protected abstract string GetVideoIdFromIntentDataString(string intentDataString);

        /// <summary>
        /// Populates the activity controls with the details from the supplied <see cref="IVideoModel"/>
        /// </summary>
        /// <param name="video">The <see cref="IVideoModel"/> to load the details into the layout for</param>
        protected abstract void LoadVideoDetails(T video);

        /// <summary>
        /// This method is where the actual work of requesting/loading the video info from the API
        /// </summary>
        /// <returns>N/A</returns>
        protected abstract Task LoadVideo();

        /// <summary>
        /// Returns the most appropriate video Thumbnail URL
        /// </summary>
        /// <remarks>
        /// This was refactored out into its own function to accomodate allowing the user to define a preferred thumbnail quality
        /// </remarks>
        /// <param name="vid">The <see cref="IVideoModel"/> whose thumbnail URL is desired</param>
        /// <returns>A URL of the thumbnail to load</returns>
        protected abstract string GetThumbnailUrl(ref T vid);

        /// <summary>
        /// The method to execute when the user wants to open the currently previewed video. Implementation will vary depending on the explicit <see cref="IVideoModel"/> type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected abstract void OpenButton_Click(object sender, System.EventArgs e);


        #endregion

        protected string videoId;

        /// <summary>
        /// The Bitmap cache where recent video thumbnails are stored
        /// </summary>
        protected LruCache MMemoryCache { get => retainedFragment.MRetainedCache; }

        /// <summary>
        /// The <see cref="RetainFragment"/> that provides continuity across activities and where in-memory caches are held
        /// </summary>
        protected RetainFragment retainedFragment;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

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

            main.LayoutTransition?.SetDuration(AndroidAnimations.LayoutTransitionType.Appearing, 150);
            main.LayoutTransition?.SetDuration(AndroidAnimations.LayoutTransitionType.Disappearing, 500);

            SetScreenSizeValues();
            SetEventHandlers();
            InitializeCache();

            await LoadVideo().ConfigureAwait(true);
        }


        /// <summary>
        /// Ensure that any outstanding <see cref="Square.Picasso.Request"/>s are canceled upon Destory 
        /// </summary>
        protected override void OnDestroy()
        {
            Square.Picasso.Picasso.With(BaseContext).CancelTag(ActivityName);
            base.OnDestroy();
        }

        /// <summary>
        /// Pause any outstanding <see cref="Square.Picasso.Request"/>s when activity is Paused
        /// </summary>
        protected override void OnPause()
        {
            Square.Picasso.Picasso.With(BaseContext).PauseTag(ActivityName);
            base.OnPause();
        }

        /// <summary>
        /// Resume any outstanding <see cref="Square.Picasso.Request"/>s when activity is Resumed 
        /// </summary>
        protected override void OnResume()
        {
            Square.Picasso.Picasso.With(BaseContext).ResumeTag(ActivityName);
            base.OnResume();
        }

        /// <summary>
        /// Ensure the recent caches are intialized and accessible, as well as double-checking the Launcher Enabled settings if they haven't been checked recently
        /// </summary>
        protected virtual void InitializeCache()
        {
            retainedFragment = RetainFragment.FindOrCreateRetainFragment(FragmentManager);
            if (retainedFragment.HavePreferencesBeenChecked)
            {
                VerifyLauncherEnabledSettings();
                retainedFragment.HavePreferencesBeenChecked = true;
            }

        }

        /// <summary>
        /// Attach the event handlers associated with the different button behaviors
        /// </summary>
        protected virtual void SetEventHandlers()
        {
            var closeButton = FindViewById<Button>(Resource.Id.closeButton);
            closeButton.Click += CloseButton_Click;

            var openButton = FindViewById<Button>(Resource.Id.button1);
            openButton.Click += OpenButton_Click;

            var aboutButton = FindViewById<ImageButton>(Resource.Id.imageButton);
            aboutButton.Click += AboutButton_Click;
        }

        /// <summary>
        /// We check this option occasionally in case the user had the launcher icon disabled and theen cleared the app data/preferences
        /// </summary>
        protected void VerifyLauncherEnabledSettings()
        {
            var componentToEnable = new ComponentName(Constants.PackageName, LauncherActivity.FullActivityName);
            var componentStatus = PackageManager.GetComponentEnabledSetting(componentToEnable);

            if (componentStatus != AndroidPM.ComponentEnabledState.Enabled || componentStatus != AndroidPM.ComponentEnabledState.Default)
            {
                // If the Launcher activity is disabled, verify that user settings supposed to be disabled 
                if (UserSettings.IsLauncherIconShown)
                    PackageManager.SetComponentEnabledSetting(componentToEnable, AndroidPM.ComponentEnabledState.Enabled, AndroidPM.ComponentEnableOption.DontKillApp);
            }
        }

        /// <summary>
        /// Loads the Video Thumbnail into the view's <see cref="ImageView"/> via <see cref="Square.Picasso.Picasso"/>, getting it from either the
        /// <see cref="LruCache"/> stored in the <see cref="RetainFragment"/> or from the supplied <paramref name="video"/> thumbnail URL.
        /// </summary>
        /// <param name="video">The <see cref="IVideoModel"/> to load the thumbnail for</param>
        protected virtual void LoadVideoThumbnail(T video)
        {
            var imgFrame = FindViewById<RelativeLayout>(Resource.Id.mediaFrame1);
            var imgHost = FindViewById<ImageView>(Resource.Id.imageView);
            var cantLoadThumbnail = GetDrawable(Resource.Drawable.CantLoadVideo);

            string cacheKey = TypePrefix + video.VideoId;

            try
            {
                if (TryGetCache(cacheKey) is Bitmap thumb)
                {
                    SetScreenSizeValues();

                    var prog = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
                    prog.Visibility = ViewStates.Invisible;

                    imgHost.Visibility = ViewStates.Visible;
                    imgHost.SetImageBitmap(thumb);

                    Square.Picasso.Picasso.With(BaseContext)
                                          .Load(GetThumbnailUrl(ref video))
                                          .Tag(ActivityName)
                                          .NoFade()
                                          .Placeholder(imgHost.Drawable)
                                          .Error(cantLoadThumbnail)
                                          .Fit()
                                          .CenterCrop()
                                          .Into(imgHost, PicassoOnSuccess, PicassoOnError);

                    Log.Info($"YTII.{ActivityName}.{nameof(LoadVideoThumbnail)}", "Thumbail Loaded From Cache");
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
                                              .Tag(ActivityName)
                                              .Error(cantLoadThumbnail)
                                              .NoFade()
                                              .Transform(new TrimBitmapHeightTransform())
                                              .Fit()
                                              .CenterCrop()
                                              .Into(imgHost, PicassoOnSuccess, PicassoOnError);

                        Log.Info($"YTII.{nameof(LoadVideoThumbnail)}", "Loaded Thumbnail into ImageView");

                        // Store the returned thubmnail in the LruCache
                        imgHost.SetWillNotCacheDrawing(false);
                        imgHost.BuildDrawingCache(false);
                        var bmc = imgHost.GetDrawingCache(false);
                        retainedFragment.MRetainedCache.Put(cacheKey, Bitmap.CreateBitmap(bmc));
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

        /// <summary>
        /// Attempts to return a cached <see cref="Bitmap"/> thumbnail that has been previous loaded. If no thumbnail is found, returns <see cref="null"/>.
        /// </summary>
        /// <param name="cacheKey">The identifying key associated with the desired cached Bitmap</param>
        /// <returns></returns>
        protected virtual object TryGetCache(string cacheKey)
        {
            try
            {
                return MMemoryCache.Get(cacheKey);
            }
            catch
            {
                return null;
            }
        }

        protected virtual void UnableToLoadVideoInfo(Exception ex = null)
        {
            var textBlock = FindViewById<TextView>(Resource.Id.textView1);
            textBlock.Text = "Unable to Load Video Information";

            var spinner = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
            spinner.Visibility = ViewStates.Invisible;

            if (ex != null)
                Log.Error($"YTII.{ActivityName}", ex.Message);
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

        /// <summary>
        /// Forces opening a URL in a browser, even if there are other apps registered as the default application for that URL
        /// </summary>
        /// <param name="url">The URL to open in the browser</param>
        protected void SendUrlToBrowser(string url)
        {
            try
            {
                var i = new Intent(Intent.ActionDefault, Uri.Parse("https://"));
                var c = i.ResolveActivity(PackageManager);
                var m = new Intent(Intent.ActionView, Uri.Parse(url));
                m.SetComponent(c);
                m.AddFlags(ActivityFlags.NewTask);

                ApplicationContext.StartActivity(m);
            }
            catch (Exception ex)
            {
                Log.Error("YTII", ex.Message);
                var toast = Toast.MakeText(this.ApplicationContext, "Failed to send intent!", ToastLength.Long);
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

        protected Size GetScreenDimensions()
        {
            var rect = new Rect();
            WindowManager.DefaultDisplay.GetRectSize(rect);
            return new Size(rect.Width(), rect.Height());
        }

        protected double GetScreenAspectRatio()
        {
            Size dims = GetScreenDimensions();
            return (double)dims.Width / (double)dims.Height;
        }

        protected int ScreenWidth { get; set; } = 480;
        protected int ScreenHeight { get; set; } = 850;

        protected int ImgWidth { get => (int)Math.Max(Math.Rint(ScreenWidth * 0.965), 480); }

        protected int ImgHeight { get => (int)Math.Rint(Math.Min(ImgWidth * ScreenAspect, ScreenHeight * 0.75)); }

        protected double ScreenAspect { get => GetScreenAspectRatio(); }

        #endregion
    }




}

