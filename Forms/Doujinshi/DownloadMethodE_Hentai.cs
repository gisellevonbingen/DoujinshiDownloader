﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodE_Hentai : DownloadMethod
    {
        public DownloadMethodE_Hentai(string name) : base(name)
        {

        }

        public override Site Site { get { return Site.E_Hentai; } }

        protected override GalleryAgent OnCreateAgent()
        {
            return new ExHentaiAgent { Account = null };
        }

    }

}