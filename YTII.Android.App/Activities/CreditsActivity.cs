#region file_header

// QuickVideoInfo - YTII.Android.App - CreditsActivity.cs
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
using Android.Content.Res;
using Android.OS;
using Java.Lang;

namespace YTII.Droid.App.Activities
{
    [Activity(Label = "Credits", MainLauncher = false, Icon = "@drawable/icon", Theme = "@style/SettingsTheme")]
    public class CreditsActivity : Activity
    {
        internal const string FullActivityName = Constants.PackageName + @".CreditsActivity";
        internal string ActivityLabel => Resources.GetString(Resource.String.about_CreditsButton);


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Credits);
            Title = ActivityLabel;
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