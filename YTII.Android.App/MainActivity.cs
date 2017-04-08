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
            get => (int)Math.Rint(Math.Min(ImgWidth * ScreenAspect, ScreenHeight * 0.60));
        }
        double ScreenAspect
        {
            get => GetScreenAspectRatio();
        }



        RetainFragment retainedFragment;
        protected void InitializeCache()
        {
            retainedFragment = RetainFragment.FindOrCreateRetainFragment(FragmentManager);

#if DEBUG
            var s = retainedFragment.MRetainedCache.Size();
            var ms = retainedFragment.MRetainedCache.MaxSize();

            var h = retainedFragment.MRetainedCache.HitCount();
            var m = retainedFragment.MRetainedCache.MissCount();

            var p = retainedFragment.MRetainedCache.PutCount();
            var c = retainedFragment.MRetainedCache.CreateCount();
            var d = retainedFragment.MRetainedCache.EvictionCount();

            Log.Debug("YTII.Cache", $"Cache Size: {s} (of {ms})");
            Log.Debug("YTII.Cache", $"Hits: {h}  |  Misses: {m}");
            Log.Debug("YTII.Cache", $"Put: {p} | Create: {c} | Evict: {d}");
#endif 
            mMemoryCache = retainedFragment.MRetainedCache;
            staticVid = retainedFragment.retainedVideo;
            //Log.Verbose($"YTII.{nameof(InitializeCache)}", $"staticVid found? = {staticVid != null}");
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            //Log.Verbose($"YTII.{nameof(OnRestoreInstanceState)}", "Saved Instance Restored");
        }
        protected override void OnPostCreate(Bundle bundle)
        {
            base.OnPostCreate(bundle);
            if (bundle?.GetBoolean("VideoLoaded") == true)
            {
                LoadSavedStateFromBundle(bundle);
            }
        }


        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            SetScreenSizeValues();
            SetEventHandlers();
            InitializeCache();

            if (bundle?.GetBoolean("VideoLoaded") == true)
            {
                //LoadSavedStateFromBundle(bundle);
            }
            else
            {
                try
                {
                    //Log.Verbose($"YTII.{nameof(OnCreate)}.ExtractIntent", "Loading Data From Intent");
                    int idIndex;
                    var da = Intent.DataString;
                    if (da != null)
                    {
                        if (da.Contains(@"watch"))
                        {
                            idIndex = da.LastIndexOf("v=") + 2;
                            videoId = da.Substring(idIndex, 11);
                        }
                        else
                        {
                            idIndex = da.LastIndexOf(@"/") + 1;
                            videoId = da.Substring(idIndex, 11);
                        }
                    }

                    var vid = await YouTubeVideoFactory.GetVideoDetailsAsync(videoId).ConfigureAwait(true);

                    if (vid != null)
                    {
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
            await Task.Run(() => System.Threading.Thread.Sleep(10));
        }

        protected Bundle loadedSavedState;
        protected void LoadSavedStateFromBundle(Bundle bundle)
        {
            try
            {
                loadedSavedState = bundle;

                videoId = bundle.GetString("VideoId");

                var videoTitle = FindViewById<TextView>(Resource.Id.textView1);
                videoTitle.Text = bundle.GetString("Title");

                var videoDuration = FindViewById<TextView>(Resource.Id.videoDuration);
                videoDuration.Text = bundle.GetString("Duration");

                var viewCount = FindViewById<TextView>(Resource.Id.viewCount);
                viewCount.Text = bundle.GetString("Views");

                var likeCount = FindViewById<TextView>(Resource.Id.likeCount);
                likeCount.Text = bundle.GetString("Likes");

                var dislikeCount = FindViewById<TextView>(Resource.Id.dislikeCount);
                dislikeCount.Text = bundle.GetString("Dislikes");

                var imgHost = FindViewById<ImageView>(Resource.Id.imageView1);

                var thumbGet = bundle.GetParcelable("thumbnail") as Bitmap;
                if (thumbGet != null)
                {
                    var newBit = Bitmap.CreateBitmap(thumbGet);
                    imgHost.SetImageBitmap(newBit);

                    var prog = FindViewById<ProgressBar>(Resource.Id.progressSpinner);

                    prog.Visibility = ViewStates.Invisible;
                    imgHost.Visibility = ViewStates.Visible;
                }
                var thumbUrl = bundle.GetString("ThumbUrl");

                Square.Picasso.Picasso.With(BaseContext)
                                      .Load(thumbUrl)
                                      .Placeholder(imgHost.Drawable)
                                      .Resize(ImgWidth, ImgHeight)
                                      .CenterCrop()
                                      .Into(imgHost, PicassoOnSuccess, PicassoOnError);

                Log.Info($"YTII.{nameof(LoadSavedStateFromBundle)}", "Restored Saved State");
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(LoadSavedStateFromBundle)}.RestoreState", ex.Message);
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
            imgHost.SetMinimumHeight(ImgHeight);
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
                viewCount.Text = video.ViewCount?.ToString();

                var likeCount = FindViewById<TextView>(Resource.Id.likeCount);
                likeCount.Text = video.LikeCount?.ToString();

                var dislikeCount = FindViewById<TextView>(Resource.Id.dislikeCount);
                dislikeCount.Text = video.DislikeCount?.ToString();

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

            try
            {
                var thumb = TryGetCache(video.VideoId) as Bitmap;

                if (thumb != null)
                {
                    //Log.Info($"YTII.{nameof(LoadVideoThumbnail)}", $"newThumb = {thumb.Width}x{thumb.Height}");

                    SetScreenSizeValues();

                    var prog = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
                    prog.Visibility = ViewStates.Invisible;
                    imgHost.Visibility = ViewStates.Visible;

                    imgHost.SetImageBitmap(thumb);

                    Square.Picasso.Picasso.With(BaseContext)
                                          .Load(GetThumbnailUrl(ref video))
                                          .Placeholder(imgHost.Drawable)
                                          .Resize(ImgWidth, ImgHeight)
                                          .CenterCrop()
                                          .Into(imgHost, PicassoOnSuccess, PicassoOnError);

                    Log.Info($"YTII.{nameof(LoadVideoThumbnail)}", "Thumbail Loaded From Cache");
                }
                else
                {
                    try
                    {
                        var thumbnailUrl = GetThumbnailUrl(ref video);
                        imgHost.SetWillNotCacheDrawing(true);

                        Square.Picasso.Picasso.With(BaseContext)
                                                  .Load(thumbnailUrl)
                                                  .Resize(ImgWidth, ImgHeight)
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

        protected override void OnDestroy()
        {
            //var imgHost = FindViewById<ImageView>(Resource.Id.imageView1);
            base.OnDestroy();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            try
            {
                if (loadedSavedState != null)
                {
                    outState.PutAll(loadedSavedState);
                }
                else
                {
                    outState.PutBoolean("VideoLoaded", true);
                    outState.PutString("VideoId", staticVid?.VideoId);
                    outState.PutString("Title", staticVid?.Title);
                    outState.PutString("Duration", staticVid?.VideoDurationString);
                    outState.PutString("Views", staticVid?.ViewCount?.ToString());
                    outState.PutString("Likes", staticVid?.LikeCount?.ToString());
                    outState.PutString("Dislikes", staticVid?.DislikeCount?.ToString());

                    outState.PutString("ThumbUrl", GetThumbnailUrl(ref staticVid));

                    var imgHost = FindViewById<ImageView>(Resource.Id.imageView1);
                    if (!imgHost.DrawingCacheEnabled && imgHost.DrawingCache == null)
                    {
                        if (imgHost.WillNotCacheDrawing())
                            imgHost.SetWillNotCacheDrawing(false);

                        imgHost.BuildDrawingCache(false);
                    }
                    var bmc = imgHost.GetDrawingCache(false);
                    outState.PutParcelable("thumbnail", Bitmap.CreateBitmap(bmc));
                }
                Log.Verbose($"YTII.{nameof(OnSaveInstanceState)}", "Stated Saved");
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(OnSaveInstanceState)}", ex.Message);
            }

            base.OnSaveInstanceState(outState);
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


        //private Bitmap GetImageBitmapFromUrl(string url)
        //{
        //    Bitmap imageBitmap = null;

        //    using (var webClient = new WebClient())
        //    {
        //        var imageBytes = webClient.DownloadData(url);
        //        if (imageBytes != null && imageBytes.Length > 0)
        //        {
        //            imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
        //        }
        //    }

        //    return imageBitmap;
        //}

    }


    class RetainFragment : Fragment
    {
        private static string TAG = "RetainFragment";
        private static LruCache _mRetainedCache;
        public LruCache MRetainedCache
        {
            get => _mRetainedCache ?? (_mRetainedCache = new LruCache(5));
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




}

