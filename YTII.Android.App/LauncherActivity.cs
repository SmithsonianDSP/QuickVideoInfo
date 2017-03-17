using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace YTII.Android.App
{
    [Activity(Name = "com.gmail.smithsoniandsp.video.previews.LauncherActivity", Label = "Video Previews for YouTube", MainLauncher = true, Icon = "@drawable/icon")]
    public class LauncherActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartActivity(typeof(AboutActivity));
        }
    }
}