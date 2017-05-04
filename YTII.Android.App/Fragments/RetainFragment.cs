#region file_header

// QuickVideoInfo - YTII.Android.App - RetainFragment.cs
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
using Android.Util;
using YTII.Droid.App.Caches;
using YTII.ModelFactory.Models;

namespace YTII.Droid.App
{
    public class RetainFragment : Fragment
    {
        const string TAG = @"RetainFragment";

        static LruCache _mRetainedCache;

        static VideoModelCache<YouTubeVideoModel> _youTubeVideoModelCache;
        static VideoModelCache<StreamableVideoModel> _streamableVideoCache;
        static VideoModelCache<VimeoVideoModel> _vimeoVideoCache;

        public YouTubeVideoModel retainedVideo { get; set; }

        public LruCache MRetainedCache => _mRetainedCache ?? (_mRetainedCache = new LruCache(5));

        public bool HavePreferencesBeenChecked { get; set; } = false;

        internal VideoModelCache<YouTubeVideoModel> YouTubeVideoModelCache => _youTubeVideoModelCache ?? (_youTubeVideoModelCache = new VideoModelCache<YouTubeVideoModel>());

        public VideoModelCache<StreamableVideoModel> StreamableVideoCache => _streamableVideoCache ?? (_streamableVideoCache = new VideoModelCache<StreamableVideoModel>());

        public VideoModelCache<VimeoVideoModel> VimeoVideoCache => _vimeoVideoCache ?? (_vimeoVideoCache = new VideoModelCache<VimeoVideoModel>());

        public static RetainFragment FindOrCreateRetainFragment(FragmentManager fm)
        {
            var fragment = fm.FindFragmentByTag<RetainFragment>(TAG);
            if (fragment == null)
            {
                fragment = new RetainFragment();
                fm.BeginTransaction().Add(fragment, TAG).Commit();
            }
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }
    }
}