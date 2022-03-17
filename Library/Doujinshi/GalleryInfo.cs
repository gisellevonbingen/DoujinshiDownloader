﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class GalleryInfo
    {
        public string GalleryUrl { get; set; } = null;
        public string Title { get; set; } = null;
        public List<string> ThumbnailUrls { get; set; } = new List<string>();
        public List<IGalleryParameterType> ParameterTypes { get; } = new List<IGalleryParameterType>();
    }

}
