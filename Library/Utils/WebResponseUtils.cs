using Giselle.Commons.Web;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class WebResponseUtils
    {
        public static HtmlDocument ReadAsDocument(this WebResponse response)
        {
            var html = response.ReadAsString();
            var document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
        }

    }

}
