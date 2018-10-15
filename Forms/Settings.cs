using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.DoujinshiDownloader.Doujinshi;
using Newtonsoft.Json.Linq;

namespace Giselle.DoujinshiDownloader
{
    public class Settings
    {
        private readonly string _Path = null;
        public string Path { get { return this._Path; } }

        private ExHentaiAccount _Account = null;
        public ExHentaiAccount Account { get { return this._Account; } set { this._Account = value; } }

        private int _Timeout = 0;
        public int Timeout { get { return this._Timeout; } set { this._Timeout = value; } }

        private int _ThreadCount = 0;
        public int ThreadCount { get { return this._ThreadCount; } set { this._ThreadCount = value; } }

        private int _RetryCount = 0;
        public int RetryCount { get { return this._RetryCount; } set { this._RetryCount = value; } }

        private string _DownloadDirectory = null;
        public string DownloadDirectory { get { return this._DownloadDirectory; } set { this._DownloadDirectory = value; } }

        private bool _DownloadCompleteAutoRemove = false;
        public bool DownloadCompleteAutoRemove { get { return this._DownloadCompleteAutoRemove; } set { this._DownloadCompleteAutoRemove = value; } }

        public Settings(string path)
        {
            this._Path = path;
        }

        public void Load()
        {
            var jObject = this.Read();

            this.Account = new ExHentaiAccount(jObject["Account"] ?? new JObject());
            this.Timeout = ((int?)jObject["Timeout"]).GetValueOrDefault(60 * 1000);
            this.ThreadCount = ((int?)jObject["ThreadCount"]).GetValueOrDefault(4);
            this.RetryCount = ((int?)jObject["RetryCount"]).GetValueOrDefault(2);

            this.DownloadDirectory = ((string)jObject["DownloadDirectory"]) ?? "Downloads";
            this.DownloadCompleteAutoRemove = ((bool?)jObject["DownloadCompleteAutoRemove"]).GetValueOrDefault(false);

            this.Save();
        }

        public void Save()
        {
            JObject jObject = new JObject();

            jObject["Account"] = this.Account.Serialize();
            jObject["Timeout"] = this.Timeout;
            jObject["ThreadCount"] = this.ThreadCount;
            jObject["RetryCount"] = this.RetryCount;
            jObject["DownloadDirectory"] = this.DownloadDirectory;
            jObject["DownloadCompleteAutoRemove"] = this.DownloadCompleteAutoRemove;

            this.Write(jObject);
        }

        private JObject Read()
        {
            string path = this.Path;

            if (File.Exists(path) == true)
            {
                return JObject.Parse(File.ReadAllText(path));
            }
            else
            {
                return new JObject();
            }

        }

        private void Write(JObject jObject)
        {
            File.WriteAllText(this.Path, jObject.ToString(Newtonsoft.Json.Formatting.None));
        }

    }

}
