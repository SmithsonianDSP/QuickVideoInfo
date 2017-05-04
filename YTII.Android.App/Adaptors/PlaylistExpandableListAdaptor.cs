#region file_header

// QuickVideoInfo - YTII.Android.App - PlaylistExpandableListAdaptor.cs
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
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Square.Picasso;
using YTII.ModelFactory.Models;

namespace YTII.Droid.App.Adaptors
{
    internal class PlaylistExpandableListAdaptor : BaseExpandableListAdapter
    {
        readonly IList<YouTubeVideoModel> _videosList;

        readonly Context context;
        ImageView imgHost;

        public int ImgMaxHeight;

        ProgressBar spinner;


        public PlaylistExpandableListAdaptor(Context context, IList<YouTubeVideoModel> videos)
        {
            this.context = context;
            _videosList = videos;
        }

        public override int GroupCount => _videosList.Count;

        public override bool HasStableIds => false;

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            var video = _videosList[groupPosition];

            if (convertView == null)
            {
                var layoutInflater = context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                convertView = layoutInflater.Inflate(Resource.Layout.playlist_item, parent, false);
            }
            convertView.Tag = video.VideoId;

            var txtListChild = convertView.FindViewById<TextView>(Resource.Id.lblListItem);
            txtListChild.Text = video.Title;

            convertView.FindViewById<TextView>(Resource.Id.viewCountItem).Text = video.ViewCountString;
            convertView.FindViewById<TextView>(Resource.Id.likeCountItem).Text = video.LikeCountString;
            convertView.FindViewById<TextView>(Resource.Id.dislikeCountItem).Text = video.DislikeCountString;

            imgHost = convertView.FindViewById<ImageView>(Resource.Id.imageViewItem);

            spinner = convertView.FindViewById<ProgressBar>(Resource.Id.progressSpinnerItem);

            spinner.Visibility = ViewStates.Gone;
            imgHost.Visibility = ViewStates.Visible;

            convertView.SetMinimumHeight(ImgMaxHeight);

            Picasso.With(context)
                   .Load(GetThumbnailUrl(ref video))
                   .Fit()
                   .CenterCrop()
                   .Into(imgHost);


            return convertView;
        }

        protected string GetThumbnailUrl(ref YouTubeVideoModel vid)
        {
            switch (UserSettings.ThumbnailQuality)
            {
                case 0:
                    return vid.MaxResThumbnailUrl
                           ?? vid.StandardThumbnailUrl
                           ?? vid.HighThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl;
                case 1:
                    return vid.StandardThumbnailUrl
                           ?? vid.HighThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl;
                case 2:
                    return vid.HighThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl;
                case 3:
                    return vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl
                           ?? vid.HighThumbnailUrl;
                case 4:
                    return vid.DefaultThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.HighThumbnailUrl;
                default:
                    return vid.StandardThumbnailUrl
                           ?? vid.HighThumbnailUrl
                           ?? vid.MediumThumbnailUrl
                           ?? vid.DefaultThumbnailUrl;
            }
        }


        protected void PicassoOnSuccess()
        {
            spinner.Visibility = ViewStates.Invisible;
            imgHost.Visibility = ViewStates.Visible;
        }

        protected void PicassoOnError()
        {
            spinner.Visibility = ViewStates.Gone;
            imgHost.Visibility = ViewStates.Visible;

            Picasso.With(context)
                   .Load(Resource.Drawable.CantLoadVideo)
                   .NoFade()
                   .Fit()
                   .CenterCrop()
                   .Into(imgHost);
        }


        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            var videoItem = _videosList[groupPosition];
            if (convertView == null)
            {
                var layoutInflater = context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                convertView = layoutInflater.Inflate(Resource.Layout.playlist_group, parent, false);
            }

            var lblListHeader = convertView.FindViewById<TextView>(Resource.Id.playlistHeader_Title);
            lblListHeader.SetTypeface(null, TypefaceStyle.Bold);
            lblListHeader.Text = videoItem.Title;

            var lblListDuration = convertView.FindViewById<TextView>(Resource.Id.playlistHeader_Duration);
            lblListDuration.Text = videoItem.VideoDurationString;

            return convertView;
        }


        public override Object GetChild(int groupPosition, int childPosition)
        {
            return _videosList[groupPosition].VideoId;
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return childPosition;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            return 1;
        }

        public override Object GetGroup(int groupPosition)
        {
            return _videosList[groupPosition].Title;
        }

        public override long GetGroupId(int groupPosition)
        {
            return groupPosition;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return true;
        }
    }

    internal class YouTubeVideoModelObj : Object
    {
        //Your adapter views to re-use
        //public TextView Title { get; set; }
    }
}