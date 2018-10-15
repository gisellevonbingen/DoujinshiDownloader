using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public struct DownloadInput : IEquatable<DownloadInput>, IComparable<DownloadInput>
    {
        public static bool operator ==(DownloadInput a, DownloadInput b)
        {
            return a.Equals(b) == true;
        }

        public static bool operator !=(DownloadInput a, DownloadInput b)
        {
            return a.Equals(b) == false;
        }

        public static DownloadInput Parse(string s)
        {
            DownloadInput value = new DownloadInput();

            if (TryParse(s, out value) == false)
            {
                throw new FormatException();
            }

            return value;
        }

        public static bool TryParse(string s, out DownloadInput result)
        {
            result = new DownloadInput();

            if (string.IsNullOrWhiteSpace(s) == true)
            {
                return false;
            }

            s = s.ToLowerInvariant().Replace(" ", "");

            if (s.StartsWith("/") == true)
            {
                s = s.Substring(1);
            }

            if (s.EndsWith("/") == true)
            {
                s = s.Substring(0, s.Length - 1);
            }

            var list = new List<Tuple<string, string>>();
            list.Add(new Tuple<string, string>("https://e-hentai.org/g/", null));
            list.Add(new Tuple<string, string>("https://exhentai.org/g/", null));
            list.Add(new Tuple<string, string>("https://hitomi.la/galleries/", ".html"));

            foreach (var site in Site.Knowns)
            {
                var prefix = site.Prefix;
                var suffix = site.Suffix;
                var splitting = s;

                if (prefix != null)
                {
                    if (splitting.StartsWith(prefix) == false)
                    {
                        continue;
                    }

                    splitting = splitting.Substring(prefix.Length);
                }

                if (suffix != null)
                {
                    if (splitting.EndsWith(suffix) == false)
                    {
                        continue;
                    }

                    splitting = splitting.Substring(0, splitting.Length - suffix.Length);
                }

                s = splitting;
            }

            var splited = s.Split('/');
            string numberToString = null;
            string keyToString = null;

            if (splited.Length == 2)
            {
                numberToString = splited[0];
                keyToString = splited[1];
            }
            else if (splited.Length == 1)
            {
                numberToString = splited[0];
            }
            else
            {
                return false;
            }

            int number = 0;

            if (int.TryParse(numberToString, out number) == false)
            {
                return false;
            }

            result.Number = number;
            result.Key = keyToString;

            return true;
        }

        private int _Number;
        public int Number { get { return this._Number; } set { this._Number = value; } }

        private string _Key;
        public string Key { get { return this._Key; } set { this._Key = value; } }

        public DownloadInput(int number) : this(number, null)
        {

        }

        public DownloadInput(int number, string key)
        {
            _Number = number;
            _Key = key;
        }

        public override string ToString()
        {
            int number = this.Number;
            string key = this.Key;

            if (key == null)
            {
                return $"{{{number}}}";
            }
            else
            {
                return $"{{{number}/{key}}}";
            }

        }

        public int CompareTo(DownloadInput other)
        {
            return this.GetHashCode().CompareTo(other.GetHashCode());
        }

        public override int GetHashCode()
        {
            var number = this.Number;
            var key = this.Key;

            int hash = 17;
            hash = hash * 17 + number;
            hash = hash * 17 + (key != null ? key.GetHashCode() : 0);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((DownloadInput)obj);
        }

        public bool Equals(DownloadInput other)
        {
            if (this.Number != other.Number)
            {
                return false;
            }

            if (string.Equals(this.Key, other.Key) == false)
            {
                return false;
            }

            return true;
        }

    }

}
