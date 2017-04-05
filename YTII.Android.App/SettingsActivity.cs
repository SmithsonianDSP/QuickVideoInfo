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
using Android.Content.PM;

namespace YTII.Android.App
{
    [Activity(Label = "Quick Video Info", MainLauncher = false, Icon = "@drawable/icon", Theme = "@style/BaseTheme")]
    public class SettingsActivity : Activity
    {
        internal const string ActivityLabel = Constants.AppTitle + " " + "Settings";
        internal const string FullActivityName = Constants.PackageName + "." + nameof(AboutActivity);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Settings);

            var iconToggle = FindViewById<Switch>(Resource.Id.toggleLauncherIcon);
            iconToggle.CheckedChange += IconToggle_CheckedChange;
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

    }
}