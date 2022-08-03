using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class ImageUtils
    {
        public static Image FromBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            try
            {
                using (var image = new MagickImage(bytes))
                {
                    return image.ToBitmap();
                }

            }
            catch
            {
                return null;
            }

        }

    }

}
