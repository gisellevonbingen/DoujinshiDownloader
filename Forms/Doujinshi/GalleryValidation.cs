﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class GalleryValidation
    {
        public IDownloadMethod Method { get; private set; } = null;
        public bool IsError { get; private set; } = false;
        public string ErrorMessage { get; private set; } = null;

        public GalleryAgent Agent { get; private set; } = null;
        public GalleryInfo Info { get; private set; } = null;
        public byte[] ThumbnailData { get; private set; } = null;

        private GalleryValidation()
        {

        }

        public static GalleryValidation CreateByError(IDownloadMethod method, string errorMessage) => new GalleryValidation
        {
            Method = method,
            IsError = true,
            ErrorMessage = errorMessage
        };

        public static GalleryValidation CreateByInfo(IDownloadMethod method, GalleryAgent agent, GalleryInfo info, byte[] thumbnailData) => new GalleryValidation
        {
            Method = method,
            IsError = false,
            Agent = agent,
            Info = info,
            ThumbnailData = thumbnailData
        };

    }

}
