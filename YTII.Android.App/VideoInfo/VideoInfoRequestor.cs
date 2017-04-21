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

    /// <summary>
    /// This class is used for communicating with the external servers to request the details and information for a specified video.
    /// </summary>
    internal static partial class VideoInfoRequestor
    {

        /* FORK TODO:
         * 
         * The following lines will need to uncommented and completed in order to implement this functionality. 
         * The actual values have not been included in the reposity for security purposes.
         * 
         * /// <summary>
         * /// This is the byte-encoded proxy URL
         * /// </summary>
         * private readonly static byte[] aurl = new byte[];
         * 
         * /// <summary>
         * /// This is the byte-encoded function key 
         * /// </summary>
         * private readonly static byte[] fkey = new byte[];
         * 
         * These values are byte-encoded in order to prevent them from being returned via a simple `string` search
         * against the binaries. 
         * 
         * /// <summary>
         * /// Returns a <see cref="HttpClient"/> that will be used for requesting the data.
         * /// </summary>
         * /// <remarks>
         * /// This method is provided for debugging purposes where providing proxy credentials/authentication is required.
         * /// </remarks>
         * static HttpClient GetHttpClient()
         * {
         *      return new HttpClient();
         * }
         * 
         */

        private static string FKey { get => Encoding.ASCII.GetString(fkey); }
        private static string FUrl { get => Encoding.ASCII.GetString(aurl); }

        /// <summary>
        /// Local storage for the app's thumbprint so it does not need to be checked every time
        /// </summary>
        internal static string Thumbprint { get; set; }

        /// <summary>
        /// Local storage for the app's package name so it does not need to be checked every time 
        /// </summary>
        internal static string PackageName { get; set; }


        /// <summary>
        /// Returns a <see cref="YouTubeVideoModel"/> for the supplied <paramref name="videoID"/>
        /// </summary>
        /// <param name="videoID">The unique YouTube VideoID</param>
        /// <returns>A <see cref="YouTubeVideoModel"/> representing the video for the associated <paramref name="videoID"/></returns>
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
