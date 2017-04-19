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

namespace YTII.Droid.App.Activities
{
    [Activity(Label = ActivityName, Theme = "@style/SettingsTheme", MainLauncher = false)]
    public class UserPreferencesActivity : Activity
    {
        const string ActivityName = "Quick Video Info Settings";

        ISharedPreferences userPrefs;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Settings);

            SetEventHandlers();
            LoadUserSettings();
        }


        static readonly string[] thumbnailOptionText = new string[] { "Max. Res.", "High (default)", "Standard", "Medium", "Lowest" };

        protected void LoadUserSettings()
        {
            var iconToggle = FindViewById<Switch>(Resource.Id.toggleLauncherIcon);
            iconToggle.Checked = UserSettings.IsLauncherIconShown;

            var thumbnailButton = FindViewById<Button>(Resource.Id.thumbnailQualityButton);
            thumbnailButton.Hint = thumbnailOptionText[UserSettings.ThumbnailQuality];
        }

        private void SetEventHandlers()
        {
            var thumbnailQualityButton = FindViewById<Button>(Resource.Id.thumbnailQualityButton);
            thumbnailQualityButton.Click += ThumbnailQualityButton_Click;

            var iconToggle = FindViewById<Switch>(Resource.Id.toggleLauncherIcon);
            iconToggle.CheckedChange += IconToggle_CheckedChange;
        }

        private void ThumbnailQualityButton_Click(object sender, EventArgs e)
        {
            var thumbnailQualityButton = FindViewById<Button>(Resource.Id.thumbnailQualityButton);
            var menu = new PopupMenu(this, thumbnailQualityButton);
            menu.Inflate(Resource.Menu.thumbnail_quality_popup_menu);

            menu.MenuItemClick += Menu_MenuItemClick;

            menu.Show();
        }

        private void Menu_MenuItemClick(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            int i = 1;

            switch (e.Item.ItemId)
            {
                case Resource.Id.thumbnail_max:
                    i = 0;
                    break;
                case Resource.Id.thumbnail_high:
                    i = 1;
                    break;
                case Resource.Id.thumbnail_standard:
                    i = 2;
                    break;
                case Resource.Id.thumbnail_medium:
                    i = 3;
                    break;
                case Resource.Id.thumbnail_lowest:
                    i = 4;
                    break;
            }

            UserSettings.SetThumbnailQuality(i);
            var thumbnailButton = FindViewById<Button>(Resource.Id.thumbnailQualityButton);
            thumbnailButton.Hint = thumbnailOptionText[i];
        }

        private void IconToggle_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
                EnableLauncherIcon();
            else
                DisableLauncherIcon();

            UserSettings.SetLauncherIconVisible(e.IsChecked);
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


    }
}