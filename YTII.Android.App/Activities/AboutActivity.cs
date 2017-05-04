#region file_header

// QuickVideoInfo - YTII.Android.App - AboutActivity.cs
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
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Java.Lang;
using YTII.Droid.App.Activities;

namespace YTII.Droid.App
{
    internal static class Constants
    {
        internal const string PackageName = @"com.gmail.smithsoniandsp.quickvideoinfo";
        internal const string AppTitle = @"Quick Video Info";
    }


    [Activity(Label = Constants.AppTitle, MainLauncher = false, Icon = "@drawable/icon", Theme = "@style/SettingsTheme")]
    public class AboutActivity : Activity
    {
        internal const string FullActivityName = Constants.PackageName + @".AboutActivity";

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

            var creditsButton = FindViewById<Button>(Resource.Id.CreditsButton);
            creditsButton.Click += CreditsButton_Click;
        }

        void CreditsButton_Click(object sender, EventArgs e)
        {
            StartActivityForResult(new Intent(this, typeof(BasePlaylistActivity)), 0);
        }

        void PrefsButton_Click(object sender, EventArgs e)
        {
            StartActivityForResult(new Intent(this, typeof(UserPreferencesActivity)), 0);
        }

        protected override void OnDestroy()
        {
            FinishAndRemoveTask();
            base.OnDestroy();
        }

        public override void FinishAndRemoveTask()
        {
            try
            {
                base.FinishAndRemoveTask();
            }
            catch (NoSuchMethodError ex)
            {
                Finish();
            }
        }
    }
}