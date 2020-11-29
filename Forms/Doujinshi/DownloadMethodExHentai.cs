﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class DownloadMethodExHentai : DownloadMethod
    {
        public DownloadMethodExHentai(string name) : base(name)
        {

        }

        public override Site Site { get { return Site.ExHentai; } }

        public override GalleryAgent CreateAgent()
        {
            var agent = new ExHentaiAgent();
            agent.Account = DoujinshiDownloader.Instance.Config.Values.Agent.ExHentaiAccount;

            return agent;
        }

    }

}