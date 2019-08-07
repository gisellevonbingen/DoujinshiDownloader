using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Giselle.Commons;
using Giselle.Commons.Web;
using Giselle.DoujinshiDownloader.Doujinshi;
using Giselle.DoujinshiDownloader.Utils;
using HtmlAgilityPack;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class ExHentaiAgent : GalleryAgent
    {
        public ExHentaiAccount Account { get; set; } = null;
        public GalleryParameterType<bool> Original { get; } = new GalleryParameterType<bool>(Guid.NewGuid(), "Original");

        public ExHentaiAgent()
        {

        }

        public override List<Site> GetSupportSites()
        {
            var sites = new List<Site>();
            sites.Add(Site.E_Hentai);
            sites.Add(Site.ExHentai);
            return sites;
        }

        public override WebRequestParameter CreateRequestParameter()
        {
            var account = this.Account;
            return this.CreateRequestParameter(account);
        }

        public WebRequestParameter CreateRequestParameter(ExHentaiAccount account)
        {
            var parameter = base.CreateRequestParameter();
            var cookies = parameter.CookieContainer = new CookieContainer();

            if (account != null)
            {
                this.SetCookie("e-hentai.org", cookies, account);
                this.SetCookie("exhentai.org", cookies, account);
            }


            return parameter;
        }

        public void SetCookie(string origin, CookieContainer cookies, ExHentaiAccount account)
        {
            var uri = new Uri($"https://{origin}/");
            var cookieOrigin = $".{origin}";
            account.MemberId.Execute(v => cookies.Add(uri, new Cookie("ipb_member_id", v, "/", cookieOrigin)));
            account.PassHash.Execute(v => cookies.Add(uri, new Cookie("ipb_pass_hash", v, "/", cookieOrigin)));
        }

        public bool CheckAccount(ExHentaiAccount account)
        {
            var parameter = this.CreateRequestParameter(account);
            parameter.Uri = "https://e-hentai.org/bounce_login.php?b=d&bt=1-1";
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var loc = response.Impl.Headers["location"];
                return string.IsNullOrWhiteSpace(loc) == false;
            }

        }

        public ImageLimit GetImageLimit(ExHentaiAccount account)
        {
            var parameter = this.CreateRequestParameter(account);
            parameter.Uri = "https://e-hentai.org/home.php";
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var document = response.ReadAsDocument();
                var stuffboxDivNode = document.DocumentNode.ChildNodes["html"].ChildNodes["body"].ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", string.Empty).Equals("stuffbox"));
                var homeboxDivNode = stuffboxDivNode.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", string.Empty).Equals("homebox"));
                var cuts = homeboxDivNode.InnerText.Cut("You are currently at ", " towards a limit of ", ". This regenerates at a rate of ", " per minute.");

                var imageLimit = new ImageLimit();
                imageLimit.Current = NumberUtils.ToInt(cuts[0]);
                imageLimit.Limit = NumberUtils.ToInt(cuts[1]);
                imageLimit.Regenerates = NumberUtils.ToInt(cuts[2]);

                return imageLimit;
            }

        }

        public override GalleryInfo GetGalleryInfo(string url)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Uri = url;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var redirect = response.Impl.Headers["Content-Disposition"];

                if (redirect != null)
                {
                    throw new ExHentaiAccountException();
                }

                var document = response.ReadAsDocument();
                var gmDivElement = document.DocumentNode.ChildNodes["html"].ChildNodes["body"].ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", string.Empty).Equals("gm"));

                var gd2Elements = gmDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gd2")).ChildNodes;
                var gnElement = gd2Elements.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gn"));
                var title = gnElement.InnerText;

                var gleftElements = gmDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gleft")).ChildNodes;
                var gd1Elements = gleftElements.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gd1")).ChildNodes;
                var thumbnailElement = gd1Elements.FirstOrDefault(n => n.Name.Equals("div"));
                var style = thumbnailElement.GetAttributeValue("style", string.Empty);
                var thumbnailUrl = style.Substring("url(", ")");

                var info = new GalleryInfo();
                info.GalleryUrl = url;
                info.Title = title;
                info.ThumbnailUrl = thumbnailUrl;
                info.ParameterTypes.Add(this.Original);

                return info;
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

        public List<string> GetGalleryImageViewsPath(string url, int page)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Uri = url + "?p=" + page;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var gdtDivElement = response.ReadAsDocument().DocumentNode.ChildNodes["html"].ChildNodes["body"].ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("gdt"));
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

        public override List<string> GetGalleryImageViewURLs(string galleryUrl, GalleryParameterValues values)
        {
            int pageCount = this.GetGalleryPageCount(galleryUrl);
            var list = new List<string>();

            for (int i = 0; i < pageCount; i++)
            {
                list.AddRange(this.GetGalleryImageViewsPath(galleryUrl, i));
            }

            return list;
        }


        public override GalleryImage GetGalleryImage(string viewUrl, GalleryParameterValues values)
        {
            var parameter = this.CreateRequestParameter();
            parameter.Uri = viewUrl;
            parameter.Method = "GET";

            using (var response = this.Explorer.Request(parameter))
            {
                var bodyElement = response.ReadAsDocument().DocumentNode.ChildNodes["html"].ChildNodes["body"];
                var mainDivElement = bodyElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i1"));

                var image = new GalleryImage();
                image.ImageUrl = this.GetGalleryImageUrl(mainDivElement, values);
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

        public string GetGalleryImageUrl(HtmlNode mainDivElement, GalleryParameterValues values)
        {
            if (values?.Get(this.Original, false) == true)
            {
                var subDivElement = mainDivElement.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("id", string.Empty).Equals("i7"));
                string url2 = HttpUtility.HtmlDecode(subDivElement?.ChildNodes["a"]?.GetAttributeValue("href", null));

                if (url2 == null)
                {
                    return null;
                }

                var parameter2 = this.CreateRequestParameter();
                parameter2.Uri = url2;
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

        public override GalleryImage ReloadImage(string requestUrl, string reloadUrl, GalleryParameterValues values)
        {
            return this.GetGalleryImage(reloadUrl, values);
        }

        public override WebRequestParameter CreateImageRequest(string imageUrl, GalleryParameterValues values)
        {
            var uri = new Uri(imageUrl);
            var fileName = uri.GetFileName();

            if (fileName.Equals("509.gif") == true)
            {
                throw new ImageRequestCreateException("ImageLimit");
            }

            return base.CreateImageRequest(imageUrl, values);
        }

    }

}
