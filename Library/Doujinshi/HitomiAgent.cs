using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Net;
using Jint;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class HitomiAgent : GalleryAgent
    {
        public static int HashLeastLength { get; } = 3;

        public Engine JintEngine { get; }
        private DownloadInput LtnInput;

        public HitomiAgent()
        {
            var engine = this.JintEngine = new Engine();
            engine.Execute("var document = {};");
            engine.Execute("document.body = {};");
            engine.Execute("document.body.appendChild = function(){};");
            engine.Execute("document.location = {};");
            engine.Execute("document.location.hostname = '';");
            engine.Execute("document.createElement = function(){ return {}; };");
            engine.Execute("document.ready = function(func){ return func(); };");
            engine.Execute("document.mouseenter = function(func){ return document; };");
            engine.Execute("document.mouseleave = function(func){ return document; };");
            engine.Execute("document.click = function(func){ return document; };");
            engine.Execute("document.each = function(func){ return document; };");
            engine.Execute("var $ = function(data){ return document; };");
            engine.SetValue("ajax", new Action<object>(ExecuteAJax));
            engine.Execute("setTimeout = function(){};");
            engine.Execute("HTMLImageElement = function(){};");
        }

        private void ExecuteAJax(dynamic req)
        {
            string url = req.url;
            var script = this.GetAsString(this.LtnInput, $"https:{url}");
            this.JintEngine.Execute(script);
        }

        public override GalleryImagePath GetGalleryImagePath(Site site, DownloadInput input, GalleryImageView view, GalleryParameterValues values)
        {
            return new GalleryImagePath() { ImageUrl = view.Url };
        }

        public override WebRequestParameter CreateThumbnailRequest(Site site, DownloadInput input, string thumbnailUrl)
        {
            var request = base.CreateThumbnailRequest(site, input, thumbnailUrl);
            request.Referer = Site.HitomiReader.ToUrl(input);

            return request;
        }

        public override WebRequestParameter CreateImageRequest(Site site, DownloadInput input, GalleryImageView view, GalleryImagePath path, GalleryParameterValues values)
        {
            var request = base.CreateImageRequest(site, input, view, path, values);
            request.Referer = Site.HitomiReader.ToUrl(input);

            return request;
        }

        public void InstallLtn(DownloadInput input)
        {
            this.LtnInput = input;

            var common = this.GetLtnCommon(input);
            common = common.Replace("$.ajax", "ajax");
            this.JintEngine.Execute(common);
        }

        public override List<GalleryImageView> GetGalleryImageViews(Site site, DownloadInput input, GalleryParameterValues values)
        {
            this.InstallLtn(input);
            var json = this.GetGalleryInfoAsJson(input);
            return this.GetGalleryImageFiles(json).Select(token => this.GetGalleryImageView(input, token)).ToList();
        }
        public string url_from_url_from_hash(int galleryId, JToken imageFileJson, string dir, string ext, string @base)
        {
            return this.JintEngine.Invoke("url_from_url_from_hash", galleryId, imageFileJson, dir, ext, @base).ToString();
        }

        public List<string> GetSmallImagePath(DownloadInput input, JToken imageFileJson)
        {
            this.InstallLtn(input);

            var @base = "tn";
            return new List<string>
            {
                this.url_from_url_from_hash(input.Number, imageFileJson, "avifbigtn", "avif", @base),
                this.url_from_url_from_hash(input.Number, imageFileJson, "avifsmallbigtn", "avif", @base),
                this.url_from_url_from_hash(input.Number, imageFileJson, "webpbigtn", "webp", @base)
            };
        }

        private GalleryImageView GetGalleryImageView(DownloadInput input, JToken imageFileJson)
        {
            var imageFile = this.ParseImageFile(imageFileJson);
            string @base;
            string ext;
            string subpath1;

            if (imageFile.HasAvif == false)
            {
                subpath1 = "webp";
                ext = ".webp";
                @base = "a";
            }
            else
            {
                subpath1 = "avif";
                ext = ".avif";
                @base = "a";
            }

            var url = this.url_from_url_from_hash(input.Number, imageFileJson, subpath1, null, @base);
            return new HitomiGalleryImageView() { Url = url.ToString(), FileName = Path.ChangeExtension(imageFileJson.Value<string>("name"), ext), ImageFile = imageFileJson };
        }

        public HitomiImageFile ParseImageFile(JToken json)
        {
            return new HitomiImageFile()
            {
                HasAvif = json.Value<int>("hasavif") > 0,
                HasAvifSmalltn = json.Value<int>("hasavifsmalltn") > 0,
                HasWebp = json.Value<int>("haswebp") > 0,
                Hash = json.Value<string>("hash"),
                Name = json.Value<string>("name"),
            };
        }

        public JObject GetGalleryInfoAsJson(DownloadInput input)
        {
            var galleryParameter = this.CreateRequestParameter();
            galleryParameter.Uri = $"https://ltn.hitomi.la/galleries/{input.Number}.js";
            galleryParameter.Referer = $"https://hitomi.la/reader/{input.Number}.html";
            galleryParameter.Method = "GET";

            using (var response = this.Explorer.Request(galleryParameter))
            {
                var script = response.ReadAsString();
                var str = Commons.StringUtils.RemovePrefix(script, "var galleryinfo = ");
                var json = JObject.Parse(str);
                return json;
            }

        }

        public IEnumerable<JToken> GetGalleryImageFiles(JToken json)
        {
            return json["files"].Values<JToken>();
        }

        public string GetAsString(DownloadInput input, string uri)
        {
            var req = this.CreateRequestParameter();
            req.Uri = uri;
            req.Referer = $"https://hitomi.la/reader/{input.Number}.html";
            req.Method = "GET";

            using (var res = this.Explorer.Request(req))
            {
                return res.ReadAsString();
            }

        }

        public string GetLtnCommon(DownloadInput input) => this.GetAsString(input, "https://ltn.hitomi.la/common.js");

        public override GalleryInfo GetGalleryInfo(Site site, DownloadInput input)
        {
            var json = this.GetGalleryInfoAsJson(input);
            var info = new GalleryInfo
            {
                GalleryUrl = site.ToUrl(input),
                Title = json.Value<string>("title"),
                ThumbnailUrls = this.GetSmallImagePath(input, this.GetGalleryImageFiles(json).FirstOrDefault())
            };

            return info;
        }

        public override GalleryImagePath ReloadImagePath(Site site, DownloadInput input, GalleryImageView _view, GalleryImagePath prev, GalleryParameterValues values)
        {
            if (_view is HitomiGalleryImageView view)
            {
                this.InstallLtn(input);
                var newView = this.GetGalleryImageView(input, view.ImageFile);
                return this.GetGalleryImagePath(site, input, newView, values);
            }
            else
            {
                throw new NotImplementedException();
            }

        }

    }

}
