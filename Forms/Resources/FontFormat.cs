﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Resources
{
    public struct FontFormat : IComparable, IComparable<FontFormat>, IEquatable<FontFormat>
    {
        public static bool operator ==(FontFormat a, FontFormat b)
        {
            return a.Equals(b) == true;
        }

        public static bool operator !=(FontFormat a, FontFormat b)
        {
            return a.Equals(b) == false;
        }

        private float _Size;
        public float Size { get { return this._Size; } }

        private FontStyle _Style;
        public FontStyle Style { get { return this._Style; } }

        public FontFormat(float size, FontStyle style)
        {
            this._Size = size;
            this._Style = style;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((FontFormat)obj);
        }

        public bool Equals(FontFormat other)
        {
            if (this.Size.Equals(other.Size) == false)
            {
                return false;
            }

            if (this.Style.Equals(other.Style) == false)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + this.Size.GetHashCode();
            hash = hash * 31 + this.Style.GetHashCode();

            return hash;
        }

        public int CompareTo(object obj)
        {
            if (obj == null || (obj is FontFormat) == false)
            {
                return 1;
            }

            return this.CompareTo((FontFormat)obj);
        }

        public int CompareTo(FontFormat other)
        {
            int c = 0;

            if ((c = this.Size.CompareTo(other.Size)) != 0)
            {
                return c;
            }

            if ((c = this.Style.CompareTo(other.Style)) != 0)
            {
                return c;
            }

            return 0;
        }

    }

}