#region file_header

// QuickVideoInfo - YTII.Android.App - VideoInfoRequestor.cs
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

using System.Text;
using System.Threading.Tasks;
using Android.Util;
using Java.Lang;
using YTII.ModelFactory.Factories;
using YTII.ModelFactory.Models;

namespace YTII.Droid.App
{
    /// <summary>
    ///     This class is used for communicating with the external servers to request the details and information for a
    ///     specified video.
    /// </summary>
    internal static partial class VideoInfoRequestor
    {
        /* FORK_TODO:
         * 
         * The following lines will need to uncommented and completed in order to implement this functionality. 
         * The actual values have not been included in the reposity for security purposes.
         * 
         * /// <summary>
         * /// This is the byte-encoded YouTube proxy function URL
         * /// </summary>
         * private readonly static byte[] youtubeVideo_aurl = new byte[];
         * 
         * /// <summary>
         * /// This is the byte-encoded YouTube Playlist proxy function URL
         * /// </summary>
         * private readonly static byte[] youtubePlaylist_aurl = new byte[];
         * 
         * /// <summary>
         * /// This is the byte-encoded Vimeo proxy function URL
         * /// </summary>
         * private readonly static byte[] vimeo_aurl = new byte[];
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

        /// <summary>
        ///     The Function Key used to Authenticate with the Video Proxy Server
        /// </summary>
        static string FKey => Encoding.ASCII.GetString(fkey);

        /// <summary>
        ///     The URL For the YouTube Video Proxy Function
        /// </summary>
        static string YouTube_FUrl => Encoding.ASCII.GetString(youtubeVideo_aurl);

        /// <summary>
        ///     The URL For the YouTube Playlist Proxy Function
        /// </summary>
        static string YouTube_Playlist_FUrl => Encoding.ASCII.GetString(youtubePlaylist_aurl);

        /// <summary>
        ///     The URL for the Vimeo Proxy Function
        /// </summary>
        static string Vimeo_FUrl => Encoding.ASCII.GetString(vimeo_aurl);

        /// <summary>
        ///     Local storage for the app's thumbprint so it does not need to be checked every time
        /// </summary>
        internal static string Thumbprint { get; set; }

        /// <summary>
        ///     Local storage for the app's package name so it does not need to be checked every time
        /// </summary>
        internal static string PackageName { get; set; }

        /// <summary>
        ///     Returns a <see cref="YouTubeVideoModel" /> for the supplied <paramref name="videoID" />
        /// </summary>
        /// <param name="videoID">The unique YouTube VideoID</param>
        /// <returns>
        ///     A <see cref="YouTubeVideoModel" /> representing the video for the associated <paramref name="videoID" />
        /// </returns>
        internal static async Task<YouTubeVideoModel> GetYouTubeVideoModel(string videoID)
        {
            var requestString = string.Format(YouTube_FUrl, videoID, FKey, Thumbprint) + "&guid=" + UserSettings.UserGuid;

            var httpClient = GetHttpClient();

            var webResponse = await httpClient.GetAsync(requestString);

            var json = await webResponse.Content.ReadAsStringAsync();

            return YouTubeVideoFactory.GetModelFromJson(json);
        }

        /// <summary>
        ///     Returns a <see cref="GetYouTubePlaylistModel" /> for the supplied <paramref name="playlistId" />
        /// </summary>
        /// <param name="playlistId">The unique YouTube playlist ID</param>
        /// <returns>
        ///     A <see cref="YouTubePlaylistModel" /> representing the playlist for the associated <paramref name="playlistId" />
        /// </returns>
        internal static async Task<YouTubePlaylistModel> GetYouTubePlaylistModel(string playlistId)
        {
            var requestString = string.Format(YouTube_Playlist_FUrl, playlistId, FKey, Thumbprint) + "&guid=" + UserSettings.UserGuid;

            var httpClient = GetHttpClient();

            var webResponse = await httpClient.GetAsync(requestString);
            var json = await webResponse.Content.ReadAsStringAsync();

            return YouTubeVideoFactory.GetPlaylistModelFromJson(json);
        }

        /// <summary>
        ///     Returns the <see cref="StreamableVideoModel" /> for the supplied <paramref name="videoID" />
        /// </summary>
        /// <param name="videoID">The unique Streamable video ID</param>
        /// <returns>
        ///     A <see cref="StreamableVideoModel" /> representing the video for the associated <paramref name="videoID" />
        /// </returns>
        internal static async Task<StreamableVideoModel> GetStreamableVideoModel(string videoID)
        {
            try
            {
                var requestString = $"https://api.streamable.com/videos/{videoID}";
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
            catch (Exception ex)
            {
                Log.Error("YTII." + nameof(GetStreamableVideoModel), ex.Message);
                return StreamableVideoFactory.GetCannotLoadVideoModel(videoID);
            }
        }

        /// <summary>
        ///     Returns a <see cref="VimeoVideoModel" /> for the supplied <paramref name="videoId" />
        /// </summary>
        /// <param name="videoId">The unique Vimeo Video ID</param>
        /// <returns>
        ///     A <see cref="VimeoVideoModel" /> representing the video for the associated <paramref name="videoId" />
        /// </returns>
        internal static async Task<VimeoVideoModel> GetVimeoVideoModel(string videoId)
        {
            var requestString = string.Format(Vimeo_FUrl, videoId, FKey, Thumbprint) + "&guid=" + UserSettings.UserGuid;

            var httpClient = GetHttpClient();

            var webResponse = await httpClient.GetAsync(requestString);

            var json = await webResponse.Content.ReadAsStringAsync();

            return VimeoVideoFactory.GetVimeoVideoModelFromJson(json);
        }
    }
}