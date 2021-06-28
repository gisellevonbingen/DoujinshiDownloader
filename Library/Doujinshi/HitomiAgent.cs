using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Web;
using Giselle.DoujinshiDownloader.Utils;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class HitomiAgent : GalleryAgent
    {
        public static int HashLeastLength { get; } = 3;

        public HitomiAgent()
        {

        }

        public override GalleryImagePath GetGalleryImage(Site site, DownloadInput input, string viewUrl, GalleryParameterValues values)
        {
            return new GalleryImagePath() { ImageUrl = viewUrl };
        }

        public override WebRequestParameter CreateImageRequest(Site site, DownloadInput input, string imageUrl, GalleryParameterValues values)
        {
            var request = base.CreateImageRequest(site, input, imageUrl, values);
            request.Referer = Site.HitomiReader.ToUrl(input);

            return request;
        }

        public override List<GalleryImageView> GetGalleryImageViews(Site site, DownloadInput input, GalleryParameterValues values)
        {
            var json = this.GetGalleryInfoAsJson(input);
            return this.GetGalleryImageFiles(json).Select(this.GetGalleryImageView).ToList();
        }

        public GalleryImageView GetGalleryImageView(HitomiImageFile file)
        {
            return new GalleryImageView() { Url = this.GetReaderImagePath(file), FileName = file.Name };
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

        public string HashToPath(string hash)
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
            var p = this.HashToPath(hash);
            return $"https://atn.hitomi.la/smalltn/{p}.jpg";
        }

        public string GetDomainPrefix(string hash, char? @base)
        {
            var c = 'b';

            if (@base.HasValue == true)
            {
                c = @base.Value;
            }

            var number_of_frontends = 3;
            var g = Convert.ToInt32(this.GetG(hash), 16);

            if (g < 0x70)
            {
                number_of_frontends = 2;
            }

            if (g < 0x49)
            {
                g = 1;
            }

            var o = g % number_of_frontends;
            var builder = new StringBuilder();
            builder.Append((char)(97 + o));
            builder.Append(c);
            return builder.ToString();
        }

        public string GetReaderImagePath(HitomiImageFile file)
        {
            var @base = new char?();
            string subpath;
            var ext = string.Empty;

            if (file.HasWebp == true)
            {
                subpath = "webp";
                ext = ".webp";
                @base = 'a';
            }
            else if (file.HasAvif == true)
            {
                subpath = "avif";
                ext = ".avif";
                @base = 'a';
            }
            else
            {
                subpath = "images";
                var extIndex = file.Name.LastIndexOf('.');

                if (extIndex != -1)
                {
                    ext = file.Name.Substring(extIndex);
                }

            }

            var hash = file.Hash;
            var path = this.HashToPath(hash);
            var prefix = this.GetDomainPrefix(hash, @base);
            return $"https://{prefix}.hitomi.la/{subpath}/{path}{ext}";
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
            var ltnMD5 = this.GetLtnCommon().GetMD5String();

            if (ltnMD5.Equals("7DE16D68B72412F4D36623E66576E23A") == false)
            {
                throw new HitomiOutdateException($"Hitomi Agent code is output, current md5 is : {ltnMD5}");
            }

            var json = this.GetGalleryInfoAsJson(input);
            var info = new GalleryInfo
            {
                GalleryUrl = site.ToUrl(input),
                Title = json.Value<string>("title"),
                ThumbnailUrl = this.GetSmallImagePath(this.GetGalleryImageFiles(json).FirstOrDefault().Hash)
            };

            return info;
        }

        public override GalleryImagePath ReloadImage(Site site, DownloadInput input, string requestUrl, string reloadUrl, GalleryParameterValues values)
        {
            throw new NotImplementedException();
        }

    }

}
