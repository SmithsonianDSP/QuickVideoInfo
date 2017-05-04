#region file_header

// QuickVideoInfo - YTII.Android.App - LauncherActivity.cs
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

using Android.App;
using Android.OS;
using Java.Lang;

namespace YTII.Droid.App
{
    [Activity(Name = FullActivityName, Label = Constants.AppTitle, MainLauncher = true, Icon = "@drawable/icon")]
    public class LauncherActivity : Activity
    {
        internal const string FullActivityName = Constants.PackageName + @".LauncherActivity";

        bool IsPaused;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (UserSettings.IsLauncherIconShown)
                StartActivity(typeof(AboutActivity));
        }

        protected override void OnPause()
        {
            base.OnPause();
            IsPaused = true;
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (IsPaused)
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
                Finish();
            }
        }
    }
}