using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Java.Util;
using Square.Picasso;
using YTII.ModelFactory.Models;

namespace YTII.Droid.App.Activities
{
    [Activity(Label = "BasePlaylistActivity")]
    public class BasePlaylistActivity : Activity
    {
        Adaptors.PlaylistExpandableListAdaptor listAdapter;

        ExpandableListView expListView;

        List<string> listDataHeader;

        Dictionary<string, IList<string>> listDataChild;

        private int ExpandedGroup = -1;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.playlist_portrait);
            SetYouTubeAuthItems();

            await LoadVideoItems();

            listAdapter = new Adaptors.PlaylistExpandableListAdaptor(this, _videosList);

            expListView = FindViewById<ExpandableListView>(Resource.Id.lvExp);

            expListView.SetAdapter(listAdapter);

            //expListView.ChildClick += ExpListView_ChildClick;
            expListView.GroupExpand += ExpListView_GroupExpand;
            expListView.GroupCollapse += ExpListView_GroupCollapse;

        }

        private IList<string> _videoIds = new List<string> { "dQw4w9WgXcQ", "ndFg58gI5mU", "2srCxPDOyi8", "YaLmmE2hVI4" };
        private IList<YouTubeVideoModel> _videosList = new List<YouTubeVideoModel>();

        private async Task LoadVideoItems()
        {
            foreach (var vid in _videoIds)
            {
                var v = await VideoInfoRequestor.GetYouTubeVideoModel(vid);
                _videosList.Add(v);
            }
        }

        protected void SetYouTubeAuthItems()
        {
            if (VideoInfoRequestor.Thumbprint == null || VideoInfoRequestor.Thumbprint == string.Empty)
                VideoInfoRequestor.Thumbprint = SignatureVerification.GetSignature(PackageManager, PackageName);

            VideoInfoRequestor.PackageName = PackageName;
        }

        private void ExpListView_GroupCollapse(object sender, ExpandableListView.GroupCollapseEventArgs e)
        {

            Picasso.With(this).CancelTag(_videosList[e.GroupPosition].VideoId);
        }

        private void ExpListView_GroupExpand(object sender, ExpandableListView.GroupExpandEventArgs e)
        {
            //Collapse any open groups   
            if (ExpandedGroup != -1 && ExpandedGroup != e.GroupPosition)
                expListView.CollapseGroup(ExpandedGroup);



            var v = expListView.FindViewWithTag(_videosList[e.GroupPosition].VideoId);
            var i = v.FindViewById<ImageView>(Resource.Id.imageViewItem);
            var s = v.FindViewById<ProgressBar>(Resource.Id.progressSpinnerItem);
            s.Visibility = ViewStates.Invisible;

            //Square.Picasso.Picasso.With(BaseContext)
            //    .Load(_videosList[e.GroupPosition].DefaultThumbnailUrl)
            //    .Tag(_videosList[e.GroupPosition].VideoId)
            //    .NoFade()
            //    .Fit()
            //    .CenterCrop()
            //    .Into(i);


            ExpandedGroup = e.GroupPosition;
        }

        private void ExpListView_ChildClick(object sender, ExpandableListView.ChildClickEventArgs e)
        {
            return;
        }
    }
}