using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using YTII.Android.App;
using YTII.APIs.Models;
using YTII.APIs.Factories;
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

namespace YTII.Android.App
{
    [Activity(Label = "Quick Video Info", Theme = "@style/CustomTheme", MainLauncher = false, Icon = "@drawable/icon")]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "http", DataHost = "*.youtube.com", DataPathPrefix = "/watch",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "https", DataHost = "*.youtube.com", DataPathPrefix = "/watch",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "http", DataHost = "youtu.be",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "https", DataHost = "youtu.be",
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    public class MainActivity : Activity
    {
        protected static YouTubeVideoModel staticVid;
        protected string videoId;
        LruCache mMemoryCache;

        RetainFragment retainedFragment;
        protected void InitializeCache()
        {
            retainedFragment = RetainFragment.FindOrCreateRetainFragment(FragmentManager);
#if DEBUG
            var s = retainedFragment.MRetainedCache.Size();
            var ms = retainedFragment.MRetainedCache.MaxSize();

            var h = retainedFragment.MRetainedCache.HitCount();
            var m = retainedFragment.MRetainedCache.MissCount();

            Log.Debug("YTII.Cache", $"Cache Size: {s} (of {ms})");
            Log.Debug("YTII.Cache", $"Hits: {h}  |  Misses: {m}");

            Log.Debug("YTII.ModelCache", $"Cache Size: {ModelCache.ItemCount}");
            Log.Debug("YTII.ModelCache", $"Cache Hits: {ModelCache.CacheHits} | Cache Misses: {ModelCache.CacheMisses}");
#endif 
            mMemoryCache = retainedFragment.MRetainedCache;

            if (retainedFragment.HavePreferencesBeenChecked)
            {
                VerifyLauncherEnabledSettings();
                retainedFragment.HavePreferencesBeenChecked = true;
            }

        }

        YouTubeModelCache ModelCache { get => retainedFragment.VideoModelCache; }

        protected string GetVideoIdFromIntentDataString(string intentDataString)
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
            YouTubeVideoFactory.AddApiAuthHeaders = true;
            YouTubeVideoFactory.ApiAuthPackageName = PackageName;
            YouTubeVideoFactory.ApiAuthSHA1 = SignatureVerification.GetSignature(PackageManager, PackageName);
        }

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            var main = FindViewById<RelativeLayout>(Resource.Id.mainLayout);
            main.LayoutTransition?.DisableTransitionType(AndroidAnimations.LayoutTransitionType.Appearing);

            SetScreenSizeValues();
            SetEventHandlers();
            InitializeCache();

            try
            {
                Log.Verbose($"YTII.{nameof(OnCreate)}.ExtractIntent", "Loading Data From Intent");

                videoId = GetVideoIdFromIntentDataString(Intent.DataString);

                YouTubeVideoModel vid;

                if (ModelCache.IsCached(videoId))
                    vid = ModelCache.GetItem(videoId);

                else
                {
#if RELEASE
                    SetYouTubeAuthItems();
#endif
                    vid = await YouTubeVideoFactory.GetVideoDetailsAsync(videoId).ConfigureAwait(true);
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

        protected void SetEventHandlers()
        {
            var closeButton = FindViewById<Button>(Resource.Id.closeButton);
            var openButton = FindViewById<Button>(Resource.Id.button1);
            var aboutButton = FindViewById<ImageButton>(Resource.Id.imageButton);

            closeButton.Click += CloseButton_Click;
            openButton.Click += OpenButton_Click;
            aboutButton.Click += AboutButton_Click;
        }

        protected void VerifyLauncherEnabledSettings()
        {
            var componentToEnable = new ComponentName(Constants.PackageName, LauncherActivity.FullActivityName);
            var componentStatus = PackageManager.GetComponentEnabledSetting(componentToEnable);

            if (componentStatus != AndroidPM.ComponentEnabledState.Enabled || componentStatus != AndroidPM.ComponentEnabledState.Default)
            {
                // If the Launcher activity is disabled, verify that user settings supposed to be disabled 
                var prefs = Application.Context.GetSharedPreferences(Constants.PackageName, FileCreationMode.Private);
                var isLauncherIconEnabled = prefs.GetBoolean("IsLaunchIconEnabled", true);
                if (isLauncherIconEnabled)
                    PackageManager.SetComponentEnabledSetting(componentToEnable, AndroidPM.ComponentEnabledState.Enabled, AndroidPM.ComponentEnableOption.DontKillApp);
            }
        }



        /// <summary>
        /// Populates the activity controls with the details from the supplied YouTubeVideo model
        /// </summary>
        /// <param name="video"></param>
        protected void LoadVideoDetails(ref YouTubeVideoModel video)
        {
            try
            {
                staticVid = video;

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

        protected void LoadVideoThumbnail(ref YouTubeVideoModel video)
        {
            var imgHost = FindViewById<ImageView>(Resource.Id.imageView1);
            var cantLoadThumbnail = GetDrawable(Resource.Drawable.CantLoadVideo);

            try
            {
                var thumb = TryGetCache(video.VideoId) as Bitmap;

                if (thumb != null)
                {
                    SetScreenSizeValues();

                    var prog = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
                    prog.Visibility = ViewStates.Invisible;
                    imgHost.Visibility = ViewStates.Visible;

                    imgHost.SetImageBitmap(thumb);

                    Square.Picasso.Picasso.With(BaseContext)
                                          .Load(GetThumbnailUrl(ref video))
                                          .NoFade()
                                          .Placeholder(imgHost.Drawable)
                                          .Error(cantLoadThumbnail)
                                          .Resize(ImgWidth, ImgHeight)
                                          .CenterInside()
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
                                              .Error(cantLoadThumbnail)
                                              .NoFade()
                                              .Resize(ImgWidth, ImgHeight)
                                              .CenterInside()
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

        protected object TryGetCache(string videoId)
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
        protected string GetThumbnailUrl(ref YouTubeVideoModel vid)
        {
            return vid.StandardThumbnailUrl
                                ?? vid.HighThumbnailUrl
                                ?? vid.MediumThumbnailUrl
                                ?? vid.DefaultThumbnailUrl;
        }

        private void UnableToLoadVideoInfo(Exception ex = null)
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

        private void PicassoOnSuccess()
        {
            try
            {
                var imgHost = FindViewById<ImageView>(Resource.Id.imageView1);
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

        private void PicassoOnError()
        {
            var spinner = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
            spinner.Visibility = ViewStates.Gone;
        }

        private void AboutButton_Click(object sender, System.EventArgs e)
        {
            StartActivity(typeof(AboutActivity));
        }

        private void CloseButton_Click(object sender, System.EventArgs e)
        {
            staticVid = null;
            retainedFragment.retainedVideo = null;
            FinishAndRemoveTask();
        }

        private void OpenButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                var i = new Intent(Intent.ActionView, Uri.Parse("vnd.youtube:" + videoId));
                i.AddFlags(ActivityFlags.NewTask);
                ApplicationContext.StartActivity(i);
                FinishAfterTransition();
                FinishAndRemoveTask();
                staticVid = null;
                retainedFragment.retainedVideo = null;
            }
            catch (Exception ex)
            {
                Log.Error("YTII", ex.Message);
                var toast = Toast.MakeText(this.ApplicationContext, "Failed to intent to YouTube App!", ToastLength.Long);
                toast.Show();
            }
        }

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

            var imgHost = FindViewById<ImageView>(Resource.Id.imageView1);
            imgHost.SetMaxHeight(ImgHeight);

            var minHeight = (int)Math.Rint(ImgHeight * 0.67);
            imgHost.SetMinimumHeight(minHeight);

            var imgFrame = FindViewById<FrameLayout>(Resource.Id.mediaFrame);
            imgFrame.SetMinimumHeight(ImgHeight);

            Log.Debug("YTII", $"Max Height: {ImgHeight} / Min Height: {minHeight}");
        }


        private Size GetScreenDimensions()
        {
            var rect = new Rect();
            WindowManager.DefaultDisplay.GetRectSize(rect);
            return new Size(rect.Width(), rect.Height());
        }

        private double GetScreenAspectRatio()
        {
            var dims = GetScreenDimensions();
            return (double)dims.Width / (double)dims.Height;
        }

        int ScreenWidth { get; set; } = 480;
        int ScreenHeight { get; set; } = 850;

        int ImgWidth
        {
            get => (int)Math.Max(Math.Rint(ScreenWidth * 0.965), 480);
        }
        int ImgHeight
        {
            get => (int)Math.Rint(Math.Min(ImgWidth * ScreenAspect, ScreenHeight * 0.55));
        }
        double ScreenAspect
        {
            get => GetScreenAspectRatio();
        }
        #endregion
    }


    class RetainFragment : Fragment
    {
        private static string TAG = "RetainFragment";
        private static LruCache _mRetainedCache;
        public LruCache MRetainedCache
        {
            get => _mRetainedCache ?? (_mRetainedCache = new LruCache(5));
        }

        private static bool _havePreferencesBeenChecked = false;
        public bool HavePreferencesBeenChecked { get => _havePreferencesBeenChecked; set => _havePreferencesBeenChecked = value; }


        private static YouTubeModelCache _videoModelCache;
        public YouTubeModelCache VideoModelCache
        {
            get => _videoModelCache ?? (_videoModelCache = new YouTubeModelCache());
        }


        public YouTubeVideoModel retainedVideo;

        public RetainFragment() { }

        public static RetainFragment FindOrCreateRetainFragment(FragmentManager fm)
        {
            RetainFragment fragment = fm.FindFragmentByTag<RetainFragment>(TAG);
            if (fragment == null)
            {
                fragment = new RetainFragment();
                fm.BeginTransaction().Add(fragment, TAG).Commit();
            }
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.RetainInstance = true;
        }
    }

    class YouTubeModelCache
    {

        public YouTubeModelCache()
        {
        }

        static private Dictionary<string, YouTubeVideoModel> _list = new Dictionary<string, YouTubeVideoModel>();
        static private Queue<string> _idOrderQueue = new Queue<string>(20);
        private const int MaxItems = 20;

        internal int ItemCount { get => _list.Count; }

        private static int _cacheHits = 0;
        internal int CacheHits { get => _cacheHits; private set => _cacheHits = value; }

        private static int _cacheMisses = 0;
        internal int CacheMisses { get => _cacheMisses; private set => _cacheMisses = value; }


        public void Add(YouTubeVideoModel item)
        {
            if (item == null || string.IsNullOrEmpty(item.VideoId))
            {
                Log.Info($"YTII.{nameof(YouTubeModelCache)}.{nameof(Add)}", $"Cannot Add Null Item/VideoId");
                return;
            }

            if (_list.Count >= MaxItems)
                _list.Remove(_idOrderQueue.Dequeue());

            if (!_idOrderQueue.Contains(item.VideoId))
                _idOrderQueue.Enqueue(item.VideoId);

            if (!_list.ContainsKey(item.VideoId))
                _list.Add(item.VideoId, item);

            Log.Info($"YTII.{nameof(YouTubeModelCache)}.{nameof(Add)}", $"Cache Item Added");
        }

        public bool IsCached(string videoId)
        {
            var isCached = _list.ContainsKey(videoId);

            if (isCached)
                CacheHits++;
            else
                CacheMisses++;

            return isCached;
        }

        public YouTubeVideoModel GetItem(string videoId)
        {
            if (!_list.ContainsKey(videoId))
                return null;

            try
            {
                var tempQueue = _idOrderQueue.Where(i => i != videoId).Reverse().ToList();
                _idOrderQueue.Clear();

                foreach (var i in tempQueue)
                    _idOrderQueue.Enqueue(i);

                _idOrderQueue.Enqueue(videoId);
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(YouTubeModelCache)}.{nameof(GetItem)}.MoveIdToBottom", ex.Message);
            }

            Log.Info($"YTII.{nameof(YouTubeModelCache)}.{nameof(GetItem)}", $"Found Cached Video Item");
            return _list[videoId];
        }


    }




}

