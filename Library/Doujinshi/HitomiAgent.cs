using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Net;
using Giselle.DoujinshiDownloader.Utils;
using Jint;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class HitomiAgent : GalleryAgent
    {
        public static int HashLeastLength { get; } = 3;

        public Engine JLintEngine { get; }

        public HitomiAgent()
        {
            var engine = this.JLintEngine = new Engine();
            engine.Execute("var document = {};");
            engine.Execute("document.location = {};");
            engine.Execute("document.location.hostname = '';");
            engine.Execute("var mock = {};");
            engine.Execute("mock.ready = function(){ return mock; };");
            engine.Execute("var $ = function(){ return mock; };");
        }

        public override GalleryImagePath GetGalleryImagePath(Site site, DownloadInput input, GalleryImageView view, GalleryParameterValues values)
        {
            return new GalleryImagePath() { ImageUrl = view.Url };
        }

        public override WebRequestParameter CreateImageRequest(Site site, DownloadInput input, GalleryImageView view, GalleryImagePath path, GalleryParameterValues values)
        {
            var request = base.CreateImageRequest(site, input, view, path, values);
            request.Referer = Site.HitomiReader.ToUrl(input);

            return request;
        }

        public void InstallLtn()
        {
            var ltn = this.GetLtnCommon();
            this.JLintEngine.Execute(ltn);
        }

        public override List<GalleryImageView> GetGalleryImageViews(Site site, DownloadInput input, GalleryParameterValues values)
        {
            this.InstallLtn();
            var json = this.GetGalleryInfoAsJson(input);
            return this.GetGalleryImageFiles(json).Select(f => this.GetGalleryImageView(f)).ToList();
        }

        private GalleryImageView GetGalleryImageView(HitomiImageFile file)
        {
            return new HitomiGalleryImageView() { Url = this.GetReaderImageViewUrl(file), FileName = file.Name, ImageFile = file };
        }

        public JObject GetGalleryInfoAsJson(DownloadInput input)
        {
            var galleryParameter = this.CreateRequestParameter();
            galleryParameter.Uri = $"https://ltn.hitomi.la/galleries/{input.Number}.js";
            galleryParameter.Method = "GET";

            using (var response = this.Explorer.Request(galleryParameter))
            {
                var script = response.ReadAsString();
                var str = Commons.StringUtils.RemovePrefix(script, "var galleryinfo = ");
                var json = JObject.Parse(str);
                return json;
            }

        }

        public string FileNameFromHash(string hash)
        {
            var length = hash.Length;

            if (length < HashLeastLength)
            {
                return hash;
            }

            return $"{hash[length - 1]}/{this.GetG(hash)}/{hash}";
        }

        public string GetG(string hash)
        {
            var length = hash.Length;

            if (length < HashLeastLength)
            {
                return hash;
            }

            return hash.Substring(length - 3, 2);
        }

        public string GetSmallImagePath(string hash)
        {
            var fileName = this.FileNameFromHash(hash);
            return $"https://atn.hitomi.la/smalltn/{fileName}.jpg";
        }

        private string GetReaderImageViewUrl(HitomiImageFile file)
        {
            var @base = new char?();
            string subpath1;
            var ext = string.Empty;

            if (file.HasWebp == true)
            {
                subpath1 = "webp";
                ext = ".webp";
                @base = 'a';
            }
            else if (file.HasAvif == true)
            {
                subpath1 = "avif";
                ext = ".avif";
                @base = 'a';
            }
            else
            {
                subpath1 = "images";
                var extIndex = file.Name.LastIndexOf('.');

                if (extIndex != -1)
                {
                    ext = file.Name.Substring(extIndex);
                }

            }

            var hash = file.Hash;
            var fileName = this.FileNameFromHash(hash);
            var preUrl = $"https://a.hitomi.la/{subpath1}/{fileName}{ext}";
            var url = this.JLintEngine.Invoke("url_from_url", preUrl, @base.ToString());
            return url.ToString();
        }

        public HitomiImageFile GetGalleryImageFile(JToken json)
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

        public IEnumerable<HitomiImageFile> GetGalleryImageFiles(JToken json)
        {
            return json["files"].Values<JToken>().Select(this.GetGalleryImageFile);
        }

        public string GetLtnCommon()
        {
            var req = this.CreateRequestParameter();
            req.Uri = "https://ltn.hitomi.la/common.js";
            req.Method = "GET";

            using (var res = this.Explorer.Request(req))
            {
                return res.ReadAsString();
            }

        }

        public override GalleryInfo GetGalleryInfo(Site site, DownloadInput input)
        {
            var json = this.GetGalleryInfoAsJson(input);
            var info = new GalleryInfo
            {
                GalleryUrl = site.ToUrl(input),
                Title = json.Value<string>("title"),
                ThumbnailUrl = this.GetSmallImagePath(this.GetGalleryImageFiles(json).FirstOrDefault().Hash)
            };

            return info;
        }

        public override GalleryImagePath ReloadImagePath(Site site, DownloadInput input, GalleryImageView _view, GalleryImagePath prev, GalleryParameterValues values)
        {
            if (_view is HitomiGalleryImageView view)
            {
                this.InstallLtn();
                var newView = this.GetGalleryImageView(view.ImageFile);
                return this.GetGalleryImagePath(site, input, newView, values);
            }
            else
            {
                throw new NotImplementedException();
            }

        }

    }

}
