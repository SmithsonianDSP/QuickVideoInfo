using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System.Threading.Tasks;
using YTII.ModelFactory.Models;


namespace YTII.Droid.App.Activities
{
    [Activity(Label = "Quick Video Info", Theme = "@style/CustomTheme", MainLauncher = false, Icon = "@drawable/icon")]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "http", DataHost = "*.streamable.com", DataPathPrefix = "", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "http", DataHost = "streamable.com", DataPathPrefix = "", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "https", DataHost = "streamable.com", DataPathPrefix = "", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    [IntentFilter(new[] { Intent.ActionView }, DataScheme = "https", DataHost = "*.streamable.com", DataPathPrefix = "", Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable })]
    public class StreamableVideoInfoActivity : MainActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var extraDetailsRow = FindViewById<GridLayout>(Resource.Id.gridDetails);
            extraDetailsRow.Visibility = ViewStates.Gone;
        }

        private Caches.VideoModelCache<StreamableVideoModel> ModelCache { get => retainedFragment.StreamableVideoCache; }


        StreamableVideoModel vid;
        protected override async Task LoadVideo()
        {
            try
            {
                videoId = GetVideoIdFromIntentDataString(Intent.DataString);

                if (ModelCache.IsCached(videoId))
                    vid = ModelCache.GetItem(videoId);
                else
                    vid = await VideoInfoRequestor.GetStreamableVideoModel(videoId).ConfigureAwait(true);

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

        protected virtual void LoadVideoDetails(ref StreamableVideoModel video)
        {
            try
            {
                var videoTitle = FindViewById<TextView>(Resource.Id.textView1);

                if (video.Title.Length > 0)
                    videoTitle.Text = video.Title;
                else
                    videoTitle.Text = "[ No Title ]";

                var videoDuration = FindViewById<TextView>(Resource.Id.videoDuration);
                videoDuration.Text = video.VideoDurationString;
                videoDuration.BringToFront();

                LoadVideoThumbnail(ref video);
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(LoadVideoDetails)}", ex.Message);
                UnableToLoadVideoInfo(ex);
            }
        }

        protected virtual void LoadVideoThumbnail(ref StreamableVideoModel video)
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
                                          .Load(video.DefaultThumbnailUrl)
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
                        var thumbnailUrl = video.DefaultThumbnailUrl;

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
                        mMemoryCache.Put(video.VideoId, Bitmap.CreateBitmap(bmc));
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



        protected override string GetVideoIdFromIntentDataString(string intentDataString)
        {
            return intentDataString.Substring(intentDataString.LastIndexOf('/'));
        }


        protected override void OpenButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                var i = new Intent(Intent.ActionDefault, Uri.Parse("https://"));
                var c = i.ResolveActivity(PackageManager);
                var m = new Intent(Intent.ActionView, Uri.Parse(vid.VideoFullUrl));
                m.SetComponent(c);
                m.AddFlags(ActivityFlags.NewTask);

                ApplicationContext.StartActivity(m);
                FinishAfterTransition();
                FinishAndRemoveTask();
            }
            catch (Exception ex)
            {
                Log.Error("YTII", ex.Message);
                var toast = Toast.MakeText(this.ApplicationContext, "Failed to send intent!", ToastLength.Long);
                toast.Show();
            }
        }

    }
}