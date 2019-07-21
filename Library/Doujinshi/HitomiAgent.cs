using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Web;
using HtmlAgilityPack;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class HitomiAgent : GalleryAgent
    {
        public bool Removed { get; set; } = false;

        public HitomiAgent()
        {

        }

        public override RequestParameter GetGalleryImageDownloadRequest(string url, DownloadGalleryParameter galleryParameter)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Method = "GET";
            parameter.URL = url;
            parameter.Referer = galleryParameter.Referer;

            return parameter;
        }

        public string GetRetval(string url)
        {
            return url;
        }

        public override List<string> GetGalleryImageViewURLs(string url)
        {
            int number = DownloadInput.Parse(url).Number;
            int bonus = (number % 10 == 1) ? 0 : (number % 2);
            var c = (char)(97 + bonus);

            var list = new List<string>();
            var parameter = this.CreateRequestParameter();
            parameter.Method = "GET";
            parameter.URL = this.ToReaderURL(url);
            parameter.Referer = url;

            using (var response = this.Explorer.Request(parameter))
            {
                var document = response.ReadToDocument();

                var nodes = document.DocumentNode.Descendants().ToArray();

                var prefix = "https://" + c.ToString() + "a.";

                var nodeInnerTexts = nodes.Where(n => n.GetAttributeValue("class", "").Equals("img-url")).Select(n => n.InnerText);

                foreach (var nodeInnerText in nodeInnerTexts)
                {
                    string imageViewURL = nodeInnerText.Replace("//g.", prefix);
                    list.Add(imageViewURL);
                }

            }

            return list;
        }

        public string ToReaderURL(string url)
        {
            return url.Replace("galleries", "reader") + "#1";
        }

        private string GetGalleryTitle(RequestParameter parameter)
        {
            using (var response = this.Explorer.Request(parameter))
            {
                if (response.Impl.StatusCode != HttpStatusCode.OK)
                {
                    throw new HitomiRemovedGalleryException();
                }

                var document = response.ReadToDocument();
                var gelleryInfo = document.DocumentNode.Descendants().FirstOrDefault(n =>
                {
                    var clazz = n.GetAttributeValue("class", string.Empty);
                    return clazz.StartsWith("gallery \n") == true && clazz.Contains("-gallery\n") == true;
                });

                if (gelleryInfo != null)
                {
                    var nodes = gelleryInfo.Descendants().ToArray();
                    var title = nodes.FirstOrDefault(n => n.Name.Equals("h1")).Descendants().FirstOrDefault(n => n.Name.Equals("a")).InnerText;
                    var artist0 = nodes.FirstOrDefault(n => n.Name.Equals("h2")).Descendants().FirstOrDefault(n => n.Name.Equals("a"))?.InnerText;

                    if (artist0 == null)
                    {
                        return title;
                    }
                    else
                    {
                        var artist = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(artist0);
                        return $"[{artist}] {title}";
                    }

                }

            }

            return null;
        }

        private string GetReaderTitle(RequestParameter parameter)
        {
            using (var response = this.Explorer.Request(parameter))
            {
                if (response.Impl.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var document = response.ReadToDocument();
                var titleTag = document.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("title", StringComparison.OrdinalIgnoreCase));

                if (titleTag != null)
                {
                    var title = titleTag.InnerText.Replace(" | Hitomi.la", "");
                    return title;
                }

            }

            return null;
        }

        public override string GetGalleryTitle(string url)
        {
            var parameter = this.CreateRequestParameter();
            parameter.URL = url;
            parameter.Method = "GET";

            if (this.Removed == false)
            {
                return this.GetGalleryTitle(parameter);
            }
            else
            {
                return this.GetReaderTitle(parameter);
            }

        }

    }

}
