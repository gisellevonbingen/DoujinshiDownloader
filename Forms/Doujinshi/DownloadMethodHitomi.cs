﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodHitomi : DownloadMethod<HitomiAgent>
    {
        public DownloadMethodHitomi(string name) : base(name)
        {

        }

        public override Site Site { get { return Site.HitomiGallery; } }

        public override HitomiAgent CreateAgent(DownloadInput downloadInput, WebRequestProvider webRequestProvider)
        {
            return new HitomiAgent(this.Site, downloadInput, webRequestProvider);
        }

    }

}