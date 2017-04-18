using Android.App;
using Android.OS;
using YTII.ModelFactory.Models;
using Android.Util;
using Android.Views;

namespace YTII.Droid.App
{
    public class RetainFragment : Fragment
    {
        private static string TAG = "RetainFragment";
        private static LruCache _mRetainedCache;
        public LruCache MRetainedCache
        {
            get => _mRetainedCache ?? (_mRetainedCache = new LruCache(5));
        }



        private static bool _havePreferencesBeenChecked = false;
        public bool HavePreferencesBeenChecked { get => _havePreferencesBeenChecked; set => _havePreferencesBeenChecked = value; }




        private static YouTubeModelCache _youTubeVideoModelCache;
        internal YouTubeModelCache YouTubeVideoModelCache
        {
            get => _youTubeVideoModelCache ?? (_youTubeVideoModelCache = new YouTubeModelCache());
        }



        private static Caches.VideoModelCache<StreamableVideoModel> _streamableVideoCache;
        public Caches.VideoModelCache<StreamableVideoModel> StreamableVideoCache
        {
            get => _streamableVideoCache ?? (_streamableVideoCache = new Caches.VideoModelCache<StreamableVideoModel>());
        }


        public YouTubeVideoModel retainedVideo;

        public RetainFragment() { }

        public static RetainFragment FindOrCreateRetainFragment(FragmentManager fm)
        {
            RetainFragment fragment = fm.FindFragmentByTag<RetainFragment>(TAG);
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
            this.RetainInstance = true;
        }
    }




}

