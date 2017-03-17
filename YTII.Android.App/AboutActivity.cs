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

namespace YTII.Android.App
{
    [Activity(Label = "Video Previews for YouTube", MainLauncher = false, Icon = "@drawable/icon", Theme = "@style/BaseTheme")]
    public class AboutActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.About);

            LoadUserSettings();


            var iconToggle = FindViewById<Switch>(Resource.Id.toggleLauncherIcon);
            iconToggle.CheckedChange += IconToggle_CheckedChange;
        }

        private void IconToggle_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
                EnableLauncherIcon();
            else
                DisableLauncherIcon();

            var prefs = Application.Context.GetSharedPreferences("com.gmail.smithsoniandsp.video.previews", FileCreationMode.Private);
            var prefEdit = prefs.Edit();
            prefEdit.PutBoolean("IsLaunchIconEnabled", e.IsChecked);
            prefEdit.Commit();
        }

        protected void DisableLauncherIcon()
        {
            var componentToDisable = new ComponentName("com.gmail.smithsoniandsp.video.previews", "com.gmail.smithsoniandsp.video.previews.LauncherActivity");

            PackageManager.SetComponentEnabledSetting(componentToDisable, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
        }
        protected void EnableLauncherIcon()
        {
            var componentToEnable = new ComponentName("com.gmail.smithsoniandsp.video.previews", "com.gmail.smithsoniandsp.video.previews.LauncherActivity");
            PackageManager.SetComponentEnabledSetting(componentToEnable, ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
        }

        protected void LoadUserSettings()
        {
            var prefs = Application.Context.GetSharedPreferences("com.gmail.smithsoniandsp.video.previews", FileCreationMode.Private);

            var isLauncherIconEnabled = prefs.GetBoolean("IsLaunchIconEnabled", true);

            var iconToggle = FindViewById<Switch>(Resource.Id.toggleLauncherIcon);
            iconToggle.Checked = isLauncherIconEnabled;

        }



    }
}