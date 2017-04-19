using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace YTII.Droid.App
{
    internal static class Constants
    {
        internal const string PackageName = "com.gmail.smithsoniandsp.quickvideoinfo";
        internal const string AppTitle = "Quick Video Info";
    }


    [Activity(Label = Constants.AppTitle, MainLauncher = false, Icon = "@drawable/icon", Theme = "@style/SettingsTheme")]
    public class AboutActivity : Activity
    {
        internal const string FullActivityName = Constants.PackageName + ".AboutActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.About);
            SetEventHandlers();
        }

        internal void SetEventHandlers()
        {
            var prefsButton = FindViewById<Button>(Resource.Id.SettingsButton);
            prefsButton.Click += PrefsButton_Click;

            //var creditsButton = FindViewById<Button>(Resource.Id.CreditsButton);
            //creditsButton.Click += CreditsButton_Click;
        }

        private void CreditsButton_Click(object sender, EventArgs e)
        {
            return;
        }

        private void PrefsButton_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(this, typeof(Activities.UserPreferencesActivity)));
        }

        protected override void OnStop()
        {
            base.OnStop();
            FinishAndRemoveTask();
        }

        protected override void OnPause()
        {
            base.OnPause();
            FinishAndRemoveTask();
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


    }
}