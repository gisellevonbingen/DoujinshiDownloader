﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Giselle.Commons;
using Giselle.Commons.Net;
using Giselle.DoujinshiDownloader.Utils;
using HtmlAgilityPack;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class ExHentaiAgent : GalleryAgent
    {
        public static Regex ImageLimitRegex { get; } = new Regex("You are currently at (?'current'\\d*) towards a limit of (?'limit'\\d*)\\.");
        public static Regex UrlRegex { get; } = new Regex("url\\((?'url'.*)\\)");
        public static Regex NlRegex { get; } = new Regex("nl\\('(?'nl'.*)'\\)");

        public static Func<WebRequestParameter> UnaryRequestParameter(Func<WebRequestParameter> func, ExHentaiAccount account)
        {
            return () => func().ConsumeOwn(o => SetCookie(o, account));
        }

        public static void SetCookie(WebRequestParameter parameter, ExHentaiAccount account)
        {
            var cookies = parameter.CookieContainer = new CookieContainer();

            if (account != null)
            {
                SetCookie(cookies, account, "e-hentai.org");
                SetCookie(cookies, account, "exhentai.org");
            }

        }

        public static void SetCookie(CookieContainer cookies, ExHentaiAccount account, string origin)
        {
            var uri = new Uri($"https://{origin}/");
            var cookieOrigin = $".{origin}";
            account?.MemberId.ConsumeOwn(v => cookies.Add(uri, new Cookie("ipb_member_id", v, "/", cookieOrigin)));
            account?.PassHash.ConsumeOwn(v => cookies.Add(uri, new Cookie("ipb_pass_hash", v, "/", cookieOrigin)));
        }

        public static bool CheckAccount(WebExplorer explorer, Func<WebRequestParameter> parameterProvider)
        {
            var parameter = parameterProvider();
            parameter.Uri = "https://e-hentai.org/bounce_login.php?b=d&bt=1-1";
            parameter.Method = "GET";

            using (var response = explorer.Request(parameter))
            {
                var loc = response.Headers["location"];
                return string.IsNullOrWhiteSpace(loc) == false;
            }

        }

        public static ImageLimit GetImageLimit(WebExplorer explorer, Func<WebRequestParameter> parameterProvider)
        {
            var parameter = parameterProvider();
            parameter.Uri = "https://e-hentai.org/home.php";
            parameter.Method = "GET";

            using (var response = explorer.Request(parameter))
            {
                var document = response.ReadAsDocument();
                var stuffboxDivNode = document.DocumentNode.ChildNodes["html"].ChildNodes["body"].ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", string.Empty).Equals("stuffbox"));
                var homeboxDivNode = stuffboxDivNode.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", string.Empty).Equals("homebox"));
                var matches = ImageLimitRegex.Match(homeboxDivNode.InnerText);

                var imageLimit = new ImageLimit
                {
                    Current = matches.Groups["current"].Value.ToInt(),
                    Limit = matches.Groups["limit"].Value.ToInt(),
                };

                return imageLimit;
            }

        }

        public ExHentaiAccount Account { get; set; } = null;
        public bool Original { get; set; } = false;

        public ExHentaiAgent(Site site, DownloadInput downloadInput, WebRequestProvider webRequestProvider) : base(site, downloadInput, webRequestProvider)
        {

        }

        public override WebRequestParameter CreateRequestParameter()
        {
            return UnaryRequestParameter(base.CreateRequestParameter, this.Account)();
        }

        public override GalleryInfo GetGalleryInfo()
        {
            var parameter = this.CreateRequestParameter();
            parameter.Uri = this.GalleryUrl;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var redirect = response.Headers["Content-Disposition"];

                if (redirect != null)
                {
                    throw new ExHentaiAccountException();
                }

                var document = response.ReadAsDocument();
                var gmDivElement = document.DocumentNode.ChildNodes["html"]?.ChildNodes["body"]?.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", string.Empty).Equals("gm"));

                if (gmDivElement == null)
                {
                    throw new ExHentaiAccountException();
                }

                var gd2Elements = gmDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gd2")).ChildNodes;
                var gnElement = gd2Elements.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gn"));
                var title = gnElement.InnerText;

                var gleftElements = gmDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gleft")).ChildNodes;
                var gd1Elements = gleftElements.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gd1")).ChildNodes;
                var thumbnailElement = gd1Elements.FirstOrDefault(n => n.Name.Equals("div"));
                var style = thumbnailElement.GetAttributeValue("style", string.Empty);
                var thumbnailUrl = UrlRegex.Match(style).Groups["url"].Value;

                return new GalleryInfo
                {
                    GalleryUrl = parameter.Uri,
                    Title = title,
                    ThumbnailUrls = new[] { thumbnailUrl },
                };

            }

        }

        public int GetGalleryPageCount(string url)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Uri = url;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var gtbDivElement = response.ReadAsDocument().DocumentNode.ChildNodes["html"].ChildNodes["body"].ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", string.Empty).Equals("gtb"));
                var pageElements = gtbDivElement.ChildNodes["table"].ChildNodes["tr"].ChildNodes.ToList();
                var lastpageElement = pageElements[pageElements.Count - 2];

                return int.Parse(lastpageElement.InnerText);
            }

        }

        public IEnumerable<GalleryImageView> GetGalleryImageViews(string url, int page)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Uri = $"{url}?p={page}";
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var gdtDivElement = response.ReadAsDocument().DocumentNode.SelectSingleNode("/html/body").ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gdt"));
                var gdtmElements = gdtDivElement.ChildNodes.Where(n => n.Name.Equals("a"));

                foreach (var element in gdtmElements)
                {
                    yield return new GalleryImageView()
                    {
                        Url = element.GetAttributeValue("href", null)
                    };

                }

            }

        }

        public override IEnumerable<GalleryImageView> GetGalleryImageViews()
        {
            var galleryUrl = this.GalleryUrl;
            var pageCount = this.GetGalleryPageCount(galleryUrl);

            for (var i = 0; i < pageCount; i++)
            {
                foreach (var view in this.GetGalleryImageViews(galleryUrl, i))
                {
                    yield return view;
                }

            }

        }

        public override GalleryImagePath GetGalleryImagePath(GalleryImageView view)
        {
            return this.GetGalleryImagePath(view.Url);
        }

        private GalleryImagePath GetGalleryImagePath(string viewUrl)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Uri = viewUrl;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var bodyElement = response.ReadAsDocument().DocumentNode.ChildNodes["html"].ChildNodes["body"];
                var mainDivElement = bodyElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i1"));

                var image = new GalleryImagePath
                {
                    ImageUrl = this.GetGalleryImageUrl(mainDivElement),
                    ReloadUrl = this.GetGalleryReloadUrl(viewUrl, mainDivElement)
                };

                return image;
            }

        }

        public string GetGalleryReloadUrl(string viewUrl, HtmlNode mainDivElement)
        {
            var subDivElement = mainDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i6"));
            var loadfailElement = subDivElement.DescendantsAndSelf().FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("loadfail"));
            var functionArgs = NlRegex.Match(loadfailElement.GetAttributeValue("onclick", string.Empty)).Groups["nl"].Value;

            return $"{viewUrl}?nl={functionArgs}";
        }

        public string GetGalleryImageUrl(HtmlNode mainDivElement)
        {
            if (this.Original == true)
            {
                var subDivElement = mainDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i6"));
                string url2 = HttpUtility.HtmlDecode(subDivElement?.ChildNodes[2].ChildNodes["a"]?.GetAttributeValue("href", null));

                if (url2 == null)
                {
                    return null;
                }

                var parameter2 = this.CreateRequestParameter();
                parameter2.Uri = url2;
                parameter2.Method = "GET";

                using (var response2 = this.Explorer.Request(parameter2))
                {
                    return response2.Headers["Location"];
                }

            }
            else
            {
                var subDivElement = mainDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i3"));
                return HttpUtility.HtmlDecode(subDivElement?.ChildNodes["a"]?.ChildNodes["img"]?.GetAttributeValue("src", null));
            }

        }

        public override GalleryImagePath ReloadImagePath(GalleryImageView view, GalleryImagePath prev)
        {
            return this.GetGalleryImagePath(prev.ReloadUrl);
        }

        public override WebRequestParameter CreateImageRequest(GalleryImageView view, GalleryImagePath path)
        {
            var uri = new Uri(path.ImageUrl);
            var fileName = uri.GetFileName();

            if (fileName.Equals("509.gif") == true)
            {
                throw new ImageRequestCreateException("ImageLimit");
            }

            return base.CreateImageRequest(view, path);
        }

    }

}
