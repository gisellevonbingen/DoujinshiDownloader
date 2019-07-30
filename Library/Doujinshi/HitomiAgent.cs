﻿using System;
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

        public override GalleryImage GetGalleryImage(string viewUrl)
        {
            return new GalleryImage() { ImageUrl = viewUrl };
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

        public override GalleryInfo GetGalleryInfo(string url)
        {
            var parameter = this.CreateRequestParameter();
            parameter.URL = url;
            parameter.Method = "GET";

            var info = new GalleryInfo();

            if (this.Removed == true)
            {
                info.Title = this.GetReaderTitle(parameter);
            }
            else
            {
                using (var response = this.Explorer.Request(parameter))
                {
                    if (response.Impl.StatusCode != HttpStatusCode.OK)
                    {
                        throw new HitomiRemovedGalleryException();
                    }

                    var document = response.ReadToDocument();

                    var infoNode = document.DocumentNode.Descendants().FirstOrDefault(n =>
                    {
                        var clazz = n.GetAttributeValue("class", string.Empty);
                        return clazz.StartsWith("gallery \n") == true && clazz.Contains("-gallery\n") == true;
                    });

                    if (infoNode != null)
                    {
                        var nodes = infoNode.Descendants().ToArray();
                        var title = nodes.FirstOrDefault(n => n.Name.Equals("h1")).Descendants().FirstOrDefault(n => n.Name.Equals("a")).InnerText;
                        var artist0 = nodes.FirstOrDefault(n => n.Name.Equals("h2")).Descendants().FirstOrDefault(n => n.Name.Equals("a"))?.InnerText;

                        if (artist0 == null)
                        {
                            info.Title = title;
                        }
                        else
                        {
                            var artist = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(artist0);
                            info.Title = $"[{artist}] {title}";
                        }

                    }

                    var coverNode = document.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.HasClass("cover"));

                    if (coverNode != null)
                    {
                        var uri = new Uri(url);
                        var coverImgNode = coverNode.Descendants().FirstOrDefault(n => n.Name.Equals("img"));
                        info.Thumbnail =  uri.Scheme + ":" + coverImgNode.GetAttributeValue("src", null);
                    }

                }

            }

            return info;
        }

        public override GalleryImage ReloadImage(string requestUrl, string reloadUrl, DownloadGalleryParameter galleryParameter)
        {
            throw new NotImplementedException();
        }

    }

}
