﻿using System;
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
        public Engine JintEngine { get; }

        public HitomiAgent(Site site, DownloadInput downloadInput, WebRequestProvider webRequestProvider) : base(site, downloadInput, webRequestProvider)
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
            var script = this.GetAsString($"https:{url}");
            this.JintEngine.Execute(script);
        }

        public override GalleryImagePath GetGalleryImagePath(GalleryImageView view)
        {
            return new GalleryImagePath() { ImageUrl = view.Url };
        }

        public override WebRequestParameter CreateThumbnailRequest(string thumbnailUrl)
        {
            var request = base.CreateThumbnailRequest(thumbnailUrl);
            request.Referer = Site.HitomiReader.ToUrl(this.DownloadInput);

            return request;
        }

        public override WebRequestParameter CreateImageRequest(GalleryImageView view, GalleryImagePath path)
        {
            var request = base.CreateImageRequest(view, path);
            request.Referer = Site.HitomiReader.ToUrl(this.DownloadInput);

            return request;
        }

        public void InstallLtn()
        {
            var common = this.GetLtnCommon();
            common = common.Replace("$.ajax", "ajax");
            this.JintEngine.Execute("location = {}");
            this.JintEngine.Execute("location.hostname = 'dev.hitomi.la'");
            this.JintEngine.Execute("window = {}");
            this.JintEngine.Execute("window.location = {}");
            this.JintEngine.Execute("window.location.href = ''");
            this.JintEngine.Execute(common);
        }

        public override IEnumerable<GalleryImageView> GetGalleryImageViews()
        {
            var json = this.GetGalleryInfoAsJson();
            return this.GetGalleryImageFiles(json).Select(token => this.GetGalleryImageView(token));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:명명 스타일", Justification = "<보류 중>")]
        public string url_from_url_from_hash(JToken imageFileJson, string dir, string ext, string @base)
        {
            return this.JintEngine.Invoke("url_from_url_from_hash", this.DownloadInput.Number, imageFileJson, dir, ext, @base).ToString();
        }

        public IEnumerable<string> GetSmallImagePath(JToken imageFileJson)
        {
            this.InstallLtn();

            var @base = "tn";
            yield return this.url_from_url_from_hash(imageFileJson, "avifbigtn", "avif", @base);
            yield return this.url_from_url_from_hash(imageFileJson, "avifsmallbigtn", "avif", @base);
            yield return this.url_from_url_from_hash(imageFileJson, "webpbigtn", "webp", @base);
        }

        private GalleryImageView GetGalleryImageView(JToken imageFileJson)
        {
            var imageFile = this.ParseImageFile(imageFileJson);
            string @base;
            string ext;
            string subpath1;

            if (imageFile.HasAvif == false)
            {
                subpath1 = "webp";
                ext = ".webp";
                @base = null;
            }
            else
            {
                subpath1 = "avif";
                ext = ".avif";
                @base = null;
            }

            var url = this.url_from_url_from_hash(imageFileJson, subpath1, null, @base);
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

        public JObject GetGalleryInfoAsJson()
        {
            var galleryParameter = this.CreateRequestParameter();
            galleryParameter.Uri = $"https://ltn.gold-usergeneratedcontent.net/galleries/{this.DownloadInput.Number}.js";
            galleryParameter.Referer = Site.HitomiReader.ToUrl(this.DownloadInput);
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

        public string GetAsString(string uri)
        {
            var req = this.CreateRequestParameter();
            req.Uri = uri;
            req.Referer = Site.HitomiReader.ToUrl(this.DownloadInput);
            req.Method = "GET";

            using (var res = this.Explorer.Request(req))
            {
                return res.ReadAsString();
            }

        }

        public string GetLtnCommon() => this.GetAsString("https://ltn.gold-usergeneratedcontent.net/common.js");

        public override GalleryInfo GetGalleryInfo()
        {
            var json = this.GetGalleryInfoAsJson();
            var info = new GalleryInfo
            {
                GalleryUrl = this.GalleryUrl,
                Title = json.Value<string>("title"),
                ThumbnailUrls = this.GetSmallImagePath(this.GetGalleryImageFiles(json).FirstOrDefault()).ToArray(),
            };

            return info;
        }

        public override GalleryImagePath ReloadImagePath(GalleryImageView _view, GalleryImagePath prev)
        {
            if (_view is HitomiGalleryImageView view)
            {
                var newView = this.GetGalleryImageView(view.ImageFile);
                return this.GetGalleryImagePath(newView);
            }
            else
            {
                throw new NotImplementedException();
            }

        }

    }

}
