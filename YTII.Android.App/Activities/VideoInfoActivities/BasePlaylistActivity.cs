#region file_header

// QuickVideoInfo - YTII.Android.App - BasePlaylistActivity.cs
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Widget;
using Java.Lang;
using YTII.Droid.App.Adaptors;
using YTII.ModelFactory.Models;

namespace YTII.Droid.App.Activities
{
    [Activity(Label = "Quick Video Info", Theme = "@style/TranslucentActivity", MainLauncher = false, Icon = "@drawable/icon")]
    public class BasePlaylistActivity : Activity
    {
        readonly IList<string> _videoIds = new List<string> { "dQw4w9WgXcQ", "ndFg58gI5mU", "2srCxPDOyi8", "YaLmmE2hVI4" };
        readonly IList<YouTubeVideoModel> _videosList = new List<YouTubeVideoModel>();

        int _expandedGroup = -1;

        ExpandableListView _expListView;
        PlaylistExpandableListAdaptor _listAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetScreenSizeValues();

            SetContentView(Resource.Layout.playlist_portrait);
            SetYouTubeAuthItems();
        }

        protected override async void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);

            await LoadVideoItems();

            _listAdapter = new PlaylistExpandableListAdaptor(BaseContext, _videosList) { ImgMaxHeight = ImgHeight };

            _expListView = FindViewById<ExpandableListView>(Resource.Id.lvExp);

            _expListView.SetAdapter(_listAdapter);

            _expListView.GroupExpand += ExpListView_GroupExpand;
            _expListView.GroupCollapse += ExpListView_GroupCollapse;
        }


        async Task LoadVideoItems()
        {
            foreach (var vid in _videoIds)
            {
                var v = await VideoInfoRequestor.GetYouTubeVideoModel(vid);
                _videosList.Add(v);
            }
        }

        protected void SetYouTubeAuthItems()
        {
            if (string.IsNullOrEmpty(VideoInfoRequestor.Thumbprint))
                VideoInfoRequestor.Thumbprint = SignatureVerification.GetSignature(PackageManager, PackageName);

            VideoInfoRequestor.PackageName = PackageName;
        }

        void ExpListView_GroupCollapse(object sender, ExpandableListView.GroupCollapseEventArgs e)
        {
            //Picasso.With(this).CancelTag(_videosList[e.GroupPosition].VideoId);
        }

        void ExpListView_GroupExpand(object sender, ExpandableListView.GroupExpandEventArgs e)
        {
            //Collapse any open groups   
            if (_expandedGroup != -1 && _expandedGroup != e.GroupPosition)
                _expListView.CollapseGroup(_expandedGroup);

            _expandedGroup = e.GroupPosition;
        }

        void ExpListView_ChildClick(object sender, ExpandableListView.ChildClickEventArgs e)
        {
        }


        #region ScreenSizeBehavior

        /// <summary>
        ///     Sets dimensions for the ImageHost frame based on the current screen dimensions
        /// </summary>
        protected void SetScreenSizeValues()
        {
            var size = GetScreenDimensions();
            ScreenWidth = size.Width;
            ScreenHeight = size.Height;
        }

        protected Size GetScreenDimensions()
        {
            var rect = new Rect();
            WindowManager.DefaultDisplay.GetRectSize(rect);
            return new Size(rect.Width(), rect.Height());
        }

        protected double GetScreenAspectRatio()
        {
            var dims = GetScreenDimensions();
            return dims.Width / (double)dims.Height;
        }

        protected int ScreenWidth { get; set; } = 480;
        protected int ScreenHeight { get; set; } = 850;

        protected int ImgWidth => (int)Math.Max(Math.Rint(ScreenWidth * 0.965), 480);

        protected int ImgHeight => (int)Math.Rint(Math.Min(ImgWidth * ScreenAspect, ScreenHeight * 0.75));

        protected double ScreenAspect => GetScreenAspectRatio();

        #endregion
    }
}