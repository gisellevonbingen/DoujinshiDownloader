﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public struct DownloadInput : IEquatable<DownloadInput>, IComparable<DownloadInput>
    {
        public static string KeyDelimiter { get; } = "/";

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
            if (TryParse(s, out var value) == false)
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
            s = s.RemovePrefix("/");
            s = s.RemoveSuffix("/");

            foreach (var site in Site.Knowns)
            {
                var prefix = site.Prefix;
                var suffix = site.Suffix;
                var splitting = s;

                if (string.IsNullOrWhiteSpace(prefix) || s.Contains(prefix) == true)
                {
                    if (string.IsNullOrWhiteSpace(suffix) || s.Contains(suffix) == true)
                    {
                        s = s.Substring(prefix, suffix);
                        break;
                    }

                }

            }

            var splited = s.Split(KeyDelimiter);
            string numberToString;
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

            if (int.TryParse(numberToString, out var number) == false)
            {
                return false;
            }

            result.Number = number;
            result.Key = keyToString;

            return true;
        }

        public int Number { get; set; }
        public string Key { get; set; }

        public DownloadInput(int number) : this(number, null)
        {

        }

        public DownloadInput(int number, string key)
        {
            this.Number = number;
            this.Key = key;
        }

        public override string ToString()
        {
            var number = this.Number;
            var key = this.Key;

            if (key == null)
            {
                return $"{number}";
            }
            else
            {
                return $"{number}/{key}";
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

            var hash = ObjectUtils.HashSeed;
            hash = hash.AccumulateHashCode(number);
            hash = hash.AccumulateHashCode(key);
            return hash;
        }

        public override bool Equals(object obj)
        {
            return obj is DownloadInput other && this.Equals(other);
        }

        public bool Equals(DownloadInput other)
        {
            if (other.GetType() != this.GetType())
            {
                return false;
            }

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
