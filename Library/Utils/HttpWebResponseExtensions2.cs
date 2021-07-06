using Giselle.Commons.Net;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class HttpWebResponseExtensions2
    {
        public static HtmlDocument ReadAsDocument(this HttpWebResponse response)
        {
            var html = response.ReadAsString();
            var document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
        }

    }

}
