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

namespace YTII.Android.App
{
    internal static class Constants
    {
        internal const string PackageName = "com.gmail.smithsoniandsp.quickvideoinfo";
        internal const string AppTitle = "Quick Video Info";
    }


    [Activity(Label = Constants.AppTitle, MainLauncher = false, Icon = "@drawable/icon", Theme = "@style/BaseTheme")]
    public class AboutActivity : Activity
    {
        internal const string FullActivityName = Constants.PackageName + ".AboutActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.About);
            LoadUserSettings();
            var iconToggle = FindViewById<Switch>(Resource.Id.toggleLauncherIcon);
            iconToggle.CheckedChange += IconToggle_CheckedChange;
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

        private void IconToggle_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
                EnableLauncherIcon();
            else
                DisableLauncherIcon();

            var prefs = Application.Context.GetSharedPreferences(Constants.PackageName, FileCreationMode.Private);
            var prefEdit = prefs.Edit();
            prefEdit.PutBoolean("IsLaunchIconEnabled", e.IsChecked);
            prefEdit.Commit();
        }

        protected void DisableLauncherIcon()
        {
            var componentToDisable = new ComponentName(Constants.PackageName, LauncherActivity.FullActivityName);
            PackageManager.SetComponentEnabledSetting(componentToDisable, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
        }
        protected void EnableLauncherIcon()
        {
            var componentToEnable = new ComponentName(Constants.PackageName, LauncherActivity.FullActivityName);
            PackageManager.SetComponentEnabledSetting(componentToEnable, ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
        }

        protected void LoadUserSettings()
        {
            var prefs = Application.Context.GetSharedPreferences(Constants.PackageName, FileCreationMode.Private);
            var isLauncherIconEnabled = prefs.GetBoolean("IsLaunchIconEnabled", true);

            var iconToggle = FindViewById<Switch>(Resource.Id.toggleLauncherIcon);
            iconToggle.Checked = isLauncherIconEnabled;

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