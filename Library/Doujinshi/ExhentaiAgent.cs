﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;
using Giselle.DoujinshiDownloader.Web;
using HtmlAgilityPack;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class ExHentaiAgent : GalleryAgent
    {
        public ExHentaiAccount Account { get; set; } = null;
        public bool Original { get; set; } = false;

        public ExHentaiAgent()
        {

        }

        public override RequestParameter CreateRequestParameter()
        {
            var account = this.Account;
            return this.CreateRequestParameter(account);
        }

        public RequestParameter CreateRequestParameter(ExHentaiAccount account)
        {
            var parameter = base.CreateRequestParameter();
            var cookies = parameter.CookieContainer = new CookieContainer();

            if (account != null)
            {
                var uri = new Uri("https://exhentai.org/");
                account.MemberId.Execute(v => cookies.Add(uri, new Cookie("ipb_member_id", v, "/", ".exhentai.org")));
                account.PassHash.Execute(v => cookies.Add(uri, new Cookie("ipb_pass_hash", v, "/", ".exhentai.org")));
            }


            return parameter;
        }

        public bool CheckAccount(ExHentaiAccount account)
        {
            var parameter = this.CreateRequestParameter(account);
            parameter.URL = "https://exhentai.org";
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var document = response.ReadToDocument();
                var title = document.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("title"));
                return title != null && title.InnerText.Equals("ExHentai.org");
            }

        }

        public override GalleryInfo GetGalleryInfo(string url)
        {
            var parameter = this.CreateRequestParameter();
            parameter.URL = url;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var redirect = response.Impl.Headers["Content-Disposition"];

                if (redirect != null)
                {
                    throw new ExHentaiAccountException();
                }

                var info = new GalleryInfo();

                var document = response.ReadToDocument();
                var gmDivElement = document.DocumentNode.ChildNodes["html"].ChildNodes["body"].ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", string.Empty).Equals("gm"));

                var gd2Elements = gmDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gd2")).ChildNodes;
                var gnElement = gd2Elements.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gn"));
                info.Title = gnElement.InnerText;

                var gleftElements = gmDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gleft")).ChildNodes;
                var gd1Elements = gleftElements.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gd1")).ChildNodes;
                var thumbnailElement = gd1Elements.FirstOrDefault(n => n.Name.Equals("div"));
                var style = thumbnailElement.GetAttributeValue("style", string.Empty);
                info.Thumbnail = style.Substring("url(", ")");

                return info;
            }

        }

        public int GetGalleryPageCount(string url)
        {
            var parameter = this.CreateRequestParameter();
            parameter.URL = url;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var gtbDivElement = response.ReadToDocument().DocumentNode.ChildNodes["html"].ChildNodes["body"].ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", string.Empty).Equals("gtb"));
                var pageElements = gtbDivElement.ChildNodes["table"].ChildNodes["tr"].ChildNodes.ToList();
                var lastpageElement = pageElements[pageElements.Count - 2];

                return int.Parse(lastpageElement.InnerText);
            }

        }

        public List<string> GetGalleryImageViewsPath(string url, int page)
        {
            var parameter = this.CreateRequestParameter();
            parameter.URL = url + "?p=" + page;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var gdtDivElement = response.ReadToDocument().DocumentNode.ChildNodes["html"].ChildNodes["body"].ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gdt"));
                var gdtmElements = gdtDivElement.ChildNodes.Where(n => n.GetAttributeValue("class", string.Empty).Equals("gdtm"));

                List<string> list = new List<string>();

                foreach (var element in gdtmElements)
                {
                    string path = element.ChildNodes["div"].ChildNodes["a"].GetAttributeValue("href", null);
                    list.Add(path);
                }

                return list;
            }

        }

        public override List<string> GetGalleryImageViewURLs(string url)
        {
            int pageCount = this.GetGalleryPageCount(url);
            var list = new List<string>();

            for (int i = 0; i < pageCount; i++)
            {
                list.AddRange(this.GetGalleryImageViewsPath(url, i));
            }

            return list;
        }


        public override GalleryImage GetGalleryImage(string viewUrl)
        {
            var parameter = this.CreateRequestParameter();
            parameter.URL = viewUrl;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var bodyElement = response.ReadToDocument().DocumentNode.ChildNodes["html"].ChildNodes["body"];
                var mainDivElement = bodyElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i1"));

                var image = new GalleryImage();
                image.ImageUrl = this.GetGalleryImageUrl(mainDivElement);
                image.ReloadUrl = this.GetGalleryReloadUrl(viewUrl, mainDivElement);

                return image;
            }

        }

        public string GetGalleryReloadUrl(string viewUrl, HtmlNode mainDivElement)
        {
            var subDivElement = mainDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i6"));
            var loadfailElement = subDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("loadfail"));
            var functionArgs = loadfailElement.GetAttributeValue("onclick", string.Empty).Substring("nl('", "')");

            return $"{viewUrl}?nl={functionArgs}";
        }

        public string GetGalleryImageUrl(HtmlNode mainDivElement)
        {
            if (this.Original == true)
            {
                var subDivElement = mainDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i7"));
                string url2 = HttpUtility.HtmlDecode(subDivElement?.ChildNodes["a"]?.GetAttributeValue("href", null));

                if (url2 == null)
                {
                    return null;
                }

                var parameter2 = this.CreateRequestParameter();
                parameter2.URL = url2;
                parameter2.Method = "GET";

                using (var response2 = this.Explorer.Request(parameter2))
                {
                    return response2.Impl.Headers["Location"];
                }

            }
            else
            {
                var subDivElement = mainDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i3"));
                return HttpUtility.HtmlDecode(subDivElement?.ChildNodes["a"]?.ChildNodes["img"]?.GetAttributeValue("src", null));
            }

        }

        public override GalleryImage ReloadImage(string requestUrl, string reloadUrl, DownloadGalleryParameter galleryParameter)
        {
            return  this.GetGalleryImage(reloadUrl);
        }

    }

}
