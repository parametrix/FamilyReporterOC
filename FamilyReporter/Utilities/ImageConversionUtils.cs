using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FamilyReporter.Utilities
{
    class ImageConversionUtils
    {
        public static BitmapSource ConvertBitmapToBitmapSource(Bitmap bmp)
        {
            if (bmp == null)
            {
                return null;
            }

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), 
                                                                                    IntPtr.Zero, 
                                                                                    Int32Rect.Empty, 
                                                                                    BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
