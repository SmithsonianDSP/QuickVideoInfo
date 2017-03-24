using Android.App;
using Android.Widget;
using Android.OS;
using YTII.Android.App;
using YTIIACL;
using XamarinForms;
using Android.Net;
using System.Collections.Generic;
using Android.Graphics;
using System.Net;
using Android.Content;
using Android.Util;
using Java.Lang;
using Android.Views;

namespace YTII.Android.App
{
    [Activity(Label = "Video Preview for YouTube", Theme = "@style/CustomTheme", MainLauncher = false, Icon = "@drawable/icon")]
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

        protected XamarinForms.Models.YoutubeItem vid;
        protected string videoId = null;
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            var closeButton = FindViewById<Button>(Resource.Id.closeButton);
            closeButton.Click += CloseButton_Click;

            var openButton = FindViewById<Button>(Resource.Id.button1);
            openButton.Click += OpenButton_Click;

            var aboutButton = FindViewById<ImageButton>(Resource.Id.imageButton);
            aboutButton.Click += AboutButton_Click;

            try
            {
                var da = Intent.DataString;
                if (da != null)
                {
                    if (da.Contains(@"watch"))
                    {
                        var idIndex = da.LastIndexOf("v=") + 2;
                        videoId = da.Substring(idIndex, 11);
                    }
                    else
                    {
                        var idIndex = da.LastIndexOf(@"/") + 1;
                        videoId = da.Substring(idIndex, 11);
                    }

                }



                var vm = new XamarinForms.ViewModels.YoutubeViewModel();
                var res = await vm.GetVideosDetailsAsync(new List<string>() { videoId });

                vid = res[0];
                var textBlock = FindViewById<TextView>(Resource.Id.textView1);
                textBlock.Text = vid.Title;

                var imgHost = FindViewById<ImageView>(Resource.Id.imageView1);

                Koush.UrlImageViewHelper.SetUrlDrawable(imgHost, vid.MaxResThumbnailUrl ??
                                                                 vid.StandardThumbnailUrl ??
                                                                 vid.HighThumbnailUrl ??
                                                                 vid.MediumThumbnailUrl ??
                                                                 vid.DefaultThumbnailUrl);


                var spinner = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
                spinner.Visibility = ViewStates.Gone;
                imgHost.Visibility = ViewStates.Visible;

            }
            catch (Exception ex)
            {
                var textBlock = FindViewById<TextView>(Resource.Id.textView1);
                textBlock.Text = "Unable to Load Video Information";
                var spinner = FindViewById<ProgressBar>(Resource.Id.progressSpinner);
                spinner.Visibility = ViewStates.Invisible;
            }
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
            }
        }



        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }

    }
}

