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

namespace YTII.Droid.App
{
    public abstract class BaseVideoInfoActivity<T> : Activity
        where T : IVideoModel
    {
        protected abstract string ActivityName { get; }
        protected abstract string GetVideoIdFromIntentDataString(string intentDataString);
        /// <summary>
        /// Populates the activity controls with the details from the supplied YouTubeVideo model
        /// </summary>
        /// <param name="video"></param>
        protected abstract void LoadVideoDetails(T video);
        protected abstract Task LoadVideo();
        /// <summary>
        /// Returns the most appropriate video Thumbnail URL
        /// </summary>
        /// <remarks>
        /// Refactored this out into its own function, for now, in anticipation of eventually allowing users to set a preferred thumbnail size
        /// </remarks>
        /// <param name="vid">The YouTubeVideoModel whose thumbnail URL is desired</param>
        /// <returns>a URL of the thumbnail to load</returns>
        protected abstract string GetThumbnailUrl(ref T vid);
        protected abstract void OpenButton_Click(object sender, System.EventArgs e);


        protected string videoId;
        protected LruCache mMemoryCache { get => retainedFragment.MRetainedCache; }
        protected abstract Caches.VideoModelCache<T> ModelCache { get; }

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Square.Picasso.Picasso.With(this).CancelTag(ActivityName);
        }

        protected override void OnPause()
        {
            base.OnPause();
            Square.Picasso.Picasso.With(this).PauseTag(ActivityName);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Square.Picasso.Picasso.With(this).ResumeTag(ActivityName);
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

        protected virtual void LoadVideoThumbnail(T video)
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

