using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public interface GalleryParameterType
    {
        Guid Id { get; }
        string Name { get; }

        bool Available(object value);
    }

    public class GalleryParameterType<T> : GalleryParameterType, IEquatable<GalleryParameterType<T>>
    {
        public Guid Id { get; }
        public string Name { get; }

        public GalleryParameterType(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public virtual bool Available(T value)
        {
            return true;
        }

        bool GalleryParameterType.Available(object value)
        {
            return value is T t ? this.Available(t) : false;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is GalleryParameterType<T> other ? this.Equals(other) : false;
        }

        public bool Equals(GalleryParameterType<T> other)
        {
            if (other == null || other.GetType().Equals(this.GetType()) == false)
            {
                return false;
            }

            if (Guid.Equals(this.Id, other.Id) == false)
            {
                return false;
            }

            return true;
        }

    }

}
