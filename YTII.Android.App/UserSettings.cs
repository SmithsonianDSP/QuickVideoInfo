#region file_header

// QuickVideoInfo - YTII.Android.App - UserSettings.cs
// 
// Licensed to the Apache Software Foundation (ASF) under one or more contributor license agreements.  
// See the NOTICE file distributed with this work for additional information regarding copyright ownership.  
// The ASF licenses this file to you under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express 
// or implied.  See the License for the specific language governing permissions and limitations under the License.
//  

#endregion

using Android.App;
using Android.Content;

namespace YTII.Droid.App
{
    internal static class UserSettings
    {
        const string LauncherIconVisibleSettingKey = @"IsLaunchIconEnabled";
        const string ThumbnailQualitySettingKey = @"ThumbnailQuality";

        static ISharedPreferences _preferences;
        internal static ISharedPreferences Preferences => _preferences ?? (_preferences = Application.Context.GetSharedPreferences(Constants.PackageName, FileCreationMode.Private));

        internal static bool IsLauncherIconShown => Preferences.GetBoolean(LauncherIconVisibleSettingKey, true);

        internal static int ThumbnailQuality => Preferences.GetInt(ThumbnailQualitySettingKey, 1);

        internal static void SetLauncherIconVisible(bool value)
        {
            var prefsEdit = Preferences.Edit();
            prefsEdit.PutBoolean(LauncherIconVisibleSettingKey, value);
            prefsEdit.Commit();
        }

        internal static void SetThumbnailQuality(int value)
        {
            var prefsEdit = Preferences.Edit();
            prefsEdit.PutInt(ThumbnailQualitySettingKey, value);
            prefsEdit.Commit();
        }
    }
}