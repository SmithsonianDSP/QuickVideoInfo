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
    [Activity(Label = ActivityLabel, MainLauncher = false, Icon = "@drawable/icon", Theme = "@style/BaseTheme")]
    public class CreditsActivity : Activity
    {
        internal const string ActivityLabel = Constants.AppTitle + " " + "Credits";
        internal const string FullActivityName = Constants.PackageName + "." + nameof(CreditsActivity);
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Credits);
            // Create your application here
        }
    }
}