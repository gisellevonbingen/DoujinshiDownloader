using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Collections;
using Giselle.Commons.Tags;
using ImageMagick;

namespace Giselle.DoujinshiDownloader.Configs
{
    public class ImageConvertType : INameTag, INamed
    {
        public static SimpleNameTags<ImageConvertType> Tags { get; } = new SimpleNameTags<ImageConvertType>();
        public static ImageConvertType Original { get; } = Tags.Register(new ImageConvertType("Original", MagickFormat.Unknown));
        //public static ImageConvertType Avif { get; } = Tags.Register(new ImageConvertType("Avif", MagickFormat.Avif));
        public static ImageConvertType WebP { get; } = Tags.Register(new ImageConvertType("WebP", MagickFormat.WebP));

        public static ImageConvertType Png { get; } = Tags.Register(new ImageConvertType("Png", MagickFormat.Png));
        public static ImageConvertType Jpg { get; } = Tags.Register(new ImageConvertType("Jpg", MagickFormat.Jpg));

        public static ImageConvertType Gif { get; } = Tags.Register(new ImageConvertType("Gif", MagickFormat.Gif));

        public string Name { get;  }
        public MagickFormat Format { get; }

        public ImageConvertType(string name, MagickFormat format)
        {
            this.Name = name;
            this.Format = format;
        }

        public override string ToString()
        {
            return this.Name;
        }

    }

}
