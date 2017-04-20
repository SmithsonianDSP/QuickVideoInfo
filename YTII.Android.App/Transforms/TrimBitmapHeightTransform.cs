using System;
using Square.Picasso;
using AndroidGraphics = Android.Graphics;


namespace YTII.Droid.App
{
    /// <summary>
    /// A simple Picasso <see cref="ITransformation"/> that crops a little bit off the top and bottom of the Bitmap so it will fit a little more nicely
    /// </summary>
    public class TrimBitmapHeightTransform : Java.Lang.Object, ITransformation
    {
        public string Key => "CustomPicassoTransform()";

        public AndroidGraphics.Bitmap Transform(AndroidGraphics.Bitmap p0)
        {
            AndroidGraphics.Bitmap result = null;

            try
            {
                int newWidth = p0.Width;
                int newHeight = (int)(p0.Height * 0.95);

                int x = (p0.Width - newWidth) / 2;
                int y = (p0.Height - newHeight) / 2;

                result = AndroidGraphics.Bitmap.CreateBitmap(source: p0,
                                                                 x: x, y: y,
                                                                 width: newWidth,
                                                                 height: newHeight);



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
