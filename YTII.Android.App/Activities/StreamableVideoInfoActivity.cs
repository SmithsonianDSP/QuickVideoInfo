using Android.App;
using Android.Content;
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
    public class StreamableVideoInfoActivity : BaseVideoInfoActivity<StreamableVideoModel>
    {
        protected override string ActivityName => nameof(StreamableVideoInfoActivity);


        protected override void OnCreate(Bundle savedInstanceState)
        {
            // FIRST NEED TO CONFIRM THIS IS A STREAMABLE.COM VIDEO URL; ALL OTHERS REDIRECT TO BROWSER


            base.OnCreate(savedInstanceState);
            var extraDetailsRow = FindViewById<GridLayout>(Resource.Id.gridDetails);
            extraDetailsRow.Visibility = ViewStates.Gone;
        }

        protected override Caches.VideoModelCache<StreamableVideoModel> ModelCache { get => retainedFragment.StreamableVideoCache; }

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

        protected override void LoadVideoDetails(StreamableVideoModel video)
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

                LoadVideoThumbnail(video);
            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(LoadVideoDetails)}", ex.Message);
                UnableToLoadVideoInfo(ex);
            }
        }

        protected override string GetVideoIdFromIntentDataString(string intentDataString)
        {
            return intentDataString.Substring(intentDataString.LastIndexOf(".com/") + 5);
        }

        protected override string GetThumbnailUrl(ref StreamableVideoModel vid)
        {
            return vid.DefaultThumbnailUrl;
        }





        protected override void OpenButton_Click(object sender, System.EventArgs e)
        {
            SendUrlToBrowser(vid.VideoFullUrl);
        }

        private void SendUrlToBrowser(string url)
        {
            try
            {
                var i = new Intent(Intent.ActionDefault, Uri.Parse("https://"));
                var c = i.ResolveActivity(PackageManager);
                var m = new Intent(Intent.ActionView, Uri.Parse(url));
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