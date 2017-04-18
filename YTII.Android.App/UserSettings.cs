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

namespace YTII.Droid.App
{
    static internal class UserSettings
    {
        private const string LauncherIconVisibleSettingKey = "IsLaunchIconEnabled";
        private const string ThumbnailQualitySettingKey = "ThumbnailQuality";


        private static ISharedPreferences _preferences;
        internal static ISharedPreferences Preferences { get => _preferences ?? (_preferences = Application.Context.GetSharedPreferences(Constants.PackageName, FileCreationMode.Private)); }

        internal static bool IsLauncherIconShown { get => Preferences.GetBoolean(LauncherIconVisibleSettingKey, true); }
        internal static void SetLauncherIconVisible(bool value)
        {
            var prefsEdit = Preferences.Edit();
            prefsEdit.PutBoolean(LauncherIconVisibleSettingKey, value);
            prefsEdit.Commit();
        }


        internal static int ThumbnailQuality { get => Preferences.GetInt(ThumbnailQualitySettingKey, 1); }

        internal static void SetThumbnailQuality(int value)
        {
            var prefsEdit = Preferences.Edit();
            prefsEdit.PutInt(ThumbnailQualitySettingKey, value);
            prefsEdit.Commit();
        }

    }
}