#region file_header

// QuickVideoInfo - YTII.Android.App - TrimBitmapHeightTransform.cs
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

using Java.Lang;
using Square.Picasso;
using AndroidGraphics = Android.Graphics;


namespace YTII.Droid.App
{
    /// <summary>
    ///     A simple Picasso <see cref="ITransformation" /> that crops a little bit off the top and bottom of the Bitmap so it
    ///     will fit a little more nicely
    /// </summary>
    public class TrimBitmapHeightTransform : Object, ITransformation
    {
        public string Key => @"CustomPicassoTransform()";

        public AndroidGraphics.Bitmap Transform(AndroidGraphics.Bitmap p0)
        {
            AndroidGraphics.Bitmap result = null;

            try
            {
                var newWidth = p0.Width;
                var newHeight = (int)(p0.Height * 0.95);

                var x = (p0.Width - newWidth) / 2;
                var y = (p0.Height - newHeight) / 2;

                result = AndroidGraphics.Bitmap.CreateBitmap(p0, x, y, newWidth, newHeight);

                return result;
            }
            catch
            {
                // If we hit any problems, just return the original 
                return p0;
            }
            finally
            {
                if (result != p0)
                    p0.Recycle();
            }
        }
    }
}