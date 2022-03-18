using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using ImageMagick;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class GalleryInfoExtensions
    {
        public static byte[] GetFirstThumbnail(this GalleryInfo info, GalleryAgent agent)
        {
            foreach (var url in info.ThumbnailUrls)
            {
                if (string.IsNullOrWhiteSpace(url) == false)
                {
                    try
                    {
                        var bytes = agent.GetGalleryThumbnail(url);

                        using (var image = new MagickImage(bytes))
                        {
                            return bytes;
                        }

                    }
                    catch
                    {

                    }

                }

            }

            return null;
        }

    }

}
