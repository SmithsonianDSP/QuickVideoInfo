using Android.App;
using Android.Widget;
using Android.OS;
using YTII.Android.App;
using YTIIACL;
using XamarinForms;
using System.Linq;
using XamarinForms.Models;
using Android.Net;
using System.Collections.Generic;
using Android.Graphics;
using System.Net;
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
        private const double AspectRatio = 1.77777777777778;

        protected YoutubeItem vid;
        protected string videoId = null;
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            var imgHost = FindViewById<ImageView>(Resource.Id.imageView1);
            var textBlock = FindViewById<TextView>(Resource.Id.textView1);

            var closeButton = FindViewById<Button>(Resource.Id.closeButton);
            var openButton = FindViewById<Button>(Resource.Id.button1);
            var aboutButton = FindViewById<ImageButton>(Resource.Id.imageButton);

            closeButton.Click += CloseButton_Click;
            openButton.Click += OpenButton_Click;
            aboutButton.Click += AboutButton_Click;
            try
            {

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

                var vm = new XamarinForms.ViewModels.YoutubeViewModel();
                var t = await vm.GetVideosDetailsAsync(new List<string>() { videoId });

                if (t.Count == 0)
                {
                    UnableToLoadVideoInfo();
                }
                else
                {
                    vid = t[0];
                    textBlock.Text = vid.Title;

                    var vidUrl = vid.MaxResThumbnailUrl ?? vid.StandardThumbnailUrl
                                    ?? vid.HighThumbnailUrl
                                    ?? vid.MediumThumbnailUrl
                                    ?? vid.DefaultThumbnailUrl;

                    var imgWidth = (int)Math.Max(Math.Rint(imgHost.Width * 0.965), 480);
                    var imgHeight = (int)Math.Rint(imgWidth / AspectRatio);

                    //Log.Error($"YTII.{nameof(OnCreate)}", $"Dimensions: {imgWidth}x{imgHeight}");


                    Square.Picasso.Picasso.With(BaseContext)
                                          .Load(vidUrl)
                                          .Resize(imgWidth, imgHeight)
                                          .CenterCrop()
                                          .Into(imgHost, PicassoOnSuccess, PicassoOnError);
                }

            }
            catch (Exception ex)
            {
                Log.Error($"YTII.{nameof(OnCreate)}", ex.Message);
                UnableToLoadVideoInfo(ex);
            }
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
            var imgHost = FindViewById<ImageView>(Resource.Id.imageView1);
            var spinner = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
            spinner.Visibility = ViewStates.Gone;
            imgHost.Visibility = ViewStates.Visible;
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
            FinishAndRemoveTask();
        }

        private void OpenButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                Intent i = new Intent(Intent.ActionView, Uri.Parse("vnd.youtube:" + videoId));
                i.AddFlags(ActivityFlags.NewTask);
                this.ApplicationContext.StartActivity(i);
                FinishAfterTransition();
                FinishAndRemoveTask();
            }
            catch (Exception ex)
            {
                Log.Error("YTII", ex.Message);
                var toast = Toast.MakeText(this.ApplicationContext, "Failed to intent to YouTube App!", ToastLength.Long);
                toast.Show();
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
}

