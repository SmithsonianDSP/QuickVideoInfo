#region file_header

// QuickVideoInfo - YTII.Android.App - UserPreferencesActivity.cs
// 
// This file is licensed to you under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express 
// or implied.  See the License for the specific language governing permissions and limitations under the License.
//  

#endregion

using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

namespace YTII.Droid.App.Activities
{
    [Activity(Label = ActivityName, Theme = "@style/SettingsTheme", MainLauncher = false)]
    public class UserPreferencesActivity : Activity
    {
        const string ActivityName = @"Quick Video Info Settings";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Settings);

            Title = Resources.GetString(Resource.String.prefs_activityTitle);

            SetEventHandlers();
            LoadUserSettings();
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);


        }

        protected void LoadUserSettings()
        {
            var iconToggle = FindViewById<Switch>(Resource.Id.toggleLauncherIcon);
            iconToggle.Checked = UserSettings.IsLauncherIconShown;

            SetMenuItemText(UserSettings.ThumbnailQuality);
        }

        void SetEventHandlers()
        {
            var thumbnailQualityButton = FindViewById<Button>(Resource.Id.thumbnailQualityButton);
            thumbnailQualityButton.Click += ThumbnailQualityButton_Click;

            var iconToggle = FindViewById<Switch>(Resource.Id.toggleLauncherIcon);
            iconToggle.CheckedChange += IconToggle_CheckedChange;
        }

        void ThumbnailQualityButton_Click(object sender, EventArgs e)
        {
            var thumbnailQualityButton = FindViewById<Button>(Resource.Id.thumbnailQualityButton);
            var menu = new PopupMenu(this, thumbnailQualityButton);

            menu.Inflate(Resource.Menu.thumbnail_quality_popup_menu);

            menu.MenuItemClick += Menu_MenuItemClick;

            menu.Show();
        }

        void Menu_MenuItemClick(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            var i = 1;

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

            SetMenuItemText(i);
            UserSettings.SetThumbnailQuality(i);
        }

        void SetMenuItemText(int value)
        {
            var thumbnailButton = FindViewById<Button>(Resource.Id.thumbnailQualityButton);
            switch (value)
            {
                case 0:
                    thumbnailButton.SetText(Resource.String.quality_max);
                    break;
                case 1:
                    thumbnailButton.SetText(Resource.String.quality_high);
                    break;
                case 2:
                    thumbnailButton.SetText(Resource.String.quality_standard);
                    break;
                case 3:
                    thumbnailButton.SetText(Resource.String.quality_medium);
                    break;
                case 4:
                    thumbnailButton.SetText(Resource.String.quality_lowest);
                    break;
            }
        }


        void IconToggle_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
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