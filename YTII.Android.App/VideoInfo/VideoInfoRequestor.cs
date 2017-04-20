using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YTII.ModelFactory.Models;
using YTII.ModelFactory.Factories;
using System.Net.Http;
using System.Threading.Tasks;

namespace YTII.Droid.App
{
    internal static partial class VideoInfoRequestor
    {


        //private readonly static byte[] fkey = new byte[];
        private static string FKey { get => Encoding.ASCII.GetString(fkey); }

        //private readonly static byte[] aurl = new byte[];
        private static string FUrl { get => Encoding.ASCII.GetString(aurl); }



        internal static string Thumbprint { get; set; }
        internal static string PackageName { get; set; }

        internal static async Task<YouTubeVideoModel> GetYouTubeVideoModel(string videoID)
        {
            string requestString = string.Format(FUrl, FKey, videoID, Thumbprint);

            var httpClient = GetHttpClient();

            var webResponse = await httpClient.GetAsync(requestString);

            var json = await webResponse.Content.ReadAsStringAsync();

            return YouTubeVideoFactory.GetModelFromJson(json);
        }

        internal static async Task<StreamableVideoModel> GetStreamableVideoModel(string videoID)
        {
            try
            {
                string requestString = $"https://api.streamable.com/videos/{videoID}";
                var httpClient = GetHttpClient();

                var webResponse = await httpClient.GetAsync(requestString);

                var json = await webResponse.Content.ReadAsStringAsync();

                if (!json.StartsWith("{"))
                {
                    var errorVideo = StreamableVideoFactory.GetCannotLoadVideoModel(videoID);
                    errorVideo.Description = json;
                    errorVideo.Title = json;
                    return errorVideo;
                }

                return StreamableVideoFactory.GetModelFromJson(json);
            }
            catch (Java.Lang.Exception ex)
            {
                Android.Util.Log.Error("YTII." + nameof(GetStreamableVideoModel), ex.Message);
                return StreamableVideoFactory.GetCannotLoadVideoModel(videoID);
            }
        }
    }



}
